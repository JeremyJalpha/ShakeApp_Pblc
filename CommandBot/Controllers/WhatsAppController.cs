using CbTsSa_Shared.DBModels;
using CbTsSa_Shared.CbTsSaConstants;
using CbTsSa_Shared.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CommandBot.Interfaces;
using CbTsSa_Shared.Models;

[ApiController]
[Route("webhook/whatsapp")]
public class WhatsAppController : ControllerBase
{
    private readonly ILogger<WhatsAppController> _logger;
    private readonly IRabbitMQInterface _rabbitMQService;
    private readonly string _webhookVerifyToken;
    private readonly string _facebookAppSecret;
    private readonly IConfiguration _configuration;
    private readonly IBackgroundTaskRunner _backgroundTaskRunner;

    public WhatsAppController(
        ILogger<WhatsAppController> logger,
        IRabbitMQInterface rabbitMQService,
        IConfiguration configuration,
        IBackgroundTaskRunner backgroundTaskRunner
    )
    {
        _logger = logger;
        _rabbitMQService = rabbitMQService;
        _webhookVerifyToken = configuration["WhatsAppBusiness:WebhookVerifyToken"] ?? throw new InvalidOperationException("WebhookVerifyToken is not set in the configuration.");
        _facebookAppSecret = configuration["WhatsAppBusiness:FacebookAppSecret"] ?? throw new InvalidOperationException("FacebookAppSecret is not set in the configuration.");
        _configuration = configuration;
        _backgroundTaskRunner = backgroundTaskRunner;
    }

    [HttpGet("root")]
    public IActionResult RootEndpoint()
    {
        return Ok("Webhook service secured with SSL!");
    }

    [HttpGet]
    public IActionResult VerifyWebhook(
        [FromQuery(Name = "hub.verify_token")] string webhookVerifyToken,
        [FromQuery(Name = "hub.challenge")] string challenge)
    {
        if (webhookVerifyToken == _webhookVerifyToken)
        {
            _logger.LogInformation("Webhook verified.");
            return Ok(challenge);
        }
        _logger.LogWarning("Error, wrong validation token.");
        return Forbid();
    }

    [HttpPost]
    public async Task<IActionResult> HandleWebhook([FromHeader(Name = "X-Hub-Signature-256")] string signature)
    {
        if (string.IsNullOrEmpty(signature))
        {
            _logger.LogWarning("Error, signature is missing.");
            return Unauthorized();
        }

        // Read the raw body as bytes
        using var memoryStream = new MemoryStream();
        await Request.Body.CopyToAsync(memoryStream);
        var bodyBytes = memoryStream.ToArray();

        // Log the raw JSON payload as a string
        var payloadString = bodyBytes;

        var computedSignature = CalculateSignatureSha256(bodyBytes, Encoding.UTF8.GetBytes(_facebookAppSecret));
        var expectedSignature = signature.StartsWith("sha256=") ? signature.Substring(7) : signature;

        if (!string.Equals(computedSignature, expectedSignature, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Error, signatures do not match.");
            return Unauthorized();
        }

        // Deserialize the payload manually
        var request = JsonSerializer.Deserialize<WebhookRequest>(payloadString);

        // Return HTTP 200 OK immediately before further processing
        _backgroundTaskRunner.Run(() => ProcessWebhook(request));

        return Ok("Success");
    }

    private Task ProcessWebhook(WebhookRequest request)
    {
        var messages = request.Entry?.FirstOrDefault()?.Changes?.FirstOrDefault()?.Value?.Messages;

        if (request == null || messages == null || messages.Count == 0)
        {
            _logger.LogWarning("Received webhook request with no messages.");
            return Task.CompletedTask;
        }

        var message = messages.FirstOrDefault();

        if (message == null)
        {
            _logger.LogWarning("Message is null.");
            return Task.CompletedTask;
        }

        string? msgOriginNumber = message.From;
        string? messageBody = message.Text?.Body;

        // Extract media handle if present
        string? mediaHandle = null;
        ChatMessageType messageType = ChatMessageType.Text;
        string? caption = null;

        if (message.Image != null)
        {
            mediaHandle = message.Image.Id; // WhatsApp media ID
            messageType = ChatMessageType.Image;
            caption = message.Image.Caption;
            messageBody = caption ?? "#campaignimage";
        }

        if (string.IsNullOrEmpty(msgOriginNumber) || string.IsNullOrEmpty(messageBody))
        {
            _logger.LogWarning("Received webhook request with missing data.");
            return Task.CompletedTask;
        }

        var chatUpdate = new ChatUpdate
        {
            From = new ApplicationUser { CellNumber = msgOriginNumber },
            Body = messageBody,
            Channel = ChatChannelType.WhatsApp,
            MessageType = messageType,
            MediaHandle = mediaHandle,
            Caption = caption
        };

        // Add tags and correlation ID
        var tags = new Dictionary<string, string>
        {
            { "channel", "whatsapp" },
            { "status", "received" }
        };

        var dispatchRequest = new ChatDispatchRequest
        {
            ChatUpdate = chatUpdate,
            CorrelationId = Guid.NewGuid(),
            Tags = tags
        };

        string serializedMessage = JsonSerializer.Serialize(dispatchRequest);

        _rabbitMQService.PublishCommand(serializedMessage);

        return Task.CompletedTask;
    }

    private static string CalculateSignatureSha256(byte[] payload, byte[] secret)
    {
        using var mac = new HMACSHA256(secret);
        var rawHmac = mac.ComputeHash(payload);
        return BitConverter.ToString(rawHmac).Replace("-", "").ToLower();
    }
}