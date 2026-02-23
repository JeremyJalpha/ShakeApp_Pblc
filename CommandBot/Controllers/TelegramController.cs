using Microsoft.AspNetCore.Mvc;
using CbTsSa_Shared.DBModels;
using System.Text.Json;
using CbTsSa_Shared.CbTsSaConstants;
using CbTsSa_Shared.Models;
using CommandBot.Interfaces;
using CbTsSa_Shared.Models.DTOs;

namespace CommandBot.Controllers
{
    [ApiController]
    [Route("webhook/telegram")]
    public class TelegramController : ControllerBase
    {
        private readonly ILogger<TelegramController> _logger;
        private readonly IRabbitMQInterface _rabbit;
        private readonly IBackgroundTaskRunner _backgroundTaskRunner;

        public TelegramController(
            ILogger<TelegramController> logger,
            IRabbitMQInterface rabbit,
            IBackgroundTaskRunner backgroundTaskRunner // new dependency
        )
        {
            _logger = logger;
            _rabbit = rabbit;
            _backgroundTaskRunner = backgroundTaskRunner;
        }

        [HttpGet("")]
        public IActionResult RootEndpoint()
        {
            return Ok("Telegram webhook active and secured.");
        }

        [HttpPost]
        public IActionResult ReceiveTelegramUpdate([FromBody] TelegramUpdate update)
        {
            if (update?.Message == null)
            {
                _logger.LogWarning("Telegram webhook received invalid update.");
                return BadRequest();
            }

            // Return 200 OK immediately, process in background
            _backgroundTaskRunner.Run(() => ProcessWebhook(update));
            return Ok("Success");
        }

        private Task ProcessWebhook(TelegramUpdate update)
        {
            var senderId = update.Message.Chat.Id.ToString();
            var messageText = update.Message.Text;
            string? mediaHandle = null;
            var messageType = ChatMessageType.Text;

            if (string.IsNullOrWhiteSpace(messageText) && update.Message.Photo != null && update.Message.Photo.Count > 0)
            {
                messageText = update.Message.Caption ?? string.Empty;
                // select highest resolution (largest width)
                var photo = update.Message.Photo.OrderByDescending(p => p.Width).First();
                mediaHandle = photo.FileId;
                messageType = ChatMessageType.Image;
                _logger.LogInformation(
                    "Telegram photo message received - FileId: {FileId}, Caption: {Caption}",
                    mediaHandle,
                    messageText);
            }

            if (string.IsNullOrWhiteSpace(senderId) || string.IsNullOrWhiteSpace(messageText))
            {
                _logger.LogWarning("Telegram webhook received empty sender or message.");
                return Task.CompletedTask;
            }

            var correlationId = Guid.NewGuid();
            var chatUpdate = new ChatUpdate
            {
                From = new ApplicationUser { CellNumber = senderId },
                Body = messageText,
                Channel = ChatChannelType.Telegram,
                MessageType = messageType,
                MediaHandle = mediaHandle
            };

            var dispatchRequest = new ChatDispatchRequest
            {
                ChatUpdate = chatUpdate,
                CorrelationId = correlationId,
                Tags = new Dictionary<string, string> { { "source", "telegram" } },
                BusinessID = null
            };

            var json = JsonSerializer.Serialize(dispatchRequest);
            _logger.LogInformation("📤 Publishing to RabbitMQ: {Json}", json);
            _rabbit.PublishCommand(json);

            return Task.CompletedTask;
        }
    }
}