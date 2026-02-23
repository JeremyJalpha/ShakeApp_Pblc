using CbTsSa_Shared.Interfaces;
using CbTsSa_Shared.Models;
using CbTsSa_Shared.CbTsSaConstants;
using WhatsappBusiness.CloudApi.Interfaces;
using WhatsappBusiness.CloudApi.Messages.Requests;
using System.Text.Json;

namespace CommandBot.Services
{
    public class WhatsAppDispatchService : IWhatsAppDispatchService
    {
        private readonly IWhatsAppBusinessClient _whatsAppClient;
        private readonly ILogger<WhatsAppDispatchService> _logger;

        public WhatsAppDispatchService(
            IWhatsAppBusinessClient whatsAppClient,
            ILogger<WhatsAppDispatchService> logger)
        {
            _whatsAppClient = whatsAppClient;
            _logger = logger;
        }

        public async Task DispatchAsync(ChatDispatchRequest dispatch)
        {
            var userId = dispatch.ChatUpdate.From.CellNumber;

            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("Skipping WhatsApp dispatch due to missing UserID.");
                return;
            }

            var message = dispatch.ChatUpdate.Body;

            try
            {
                // Check if this is an image message
                if (!string.IsNullOrWhiteSpace(dispatch.ChatUpdate.MediaHandle) && 
                    dispatch.ChatUpdate.MessageType == ChatMessageType.Image)
                {
                    var imageMessageRequest = new ImageMessageByIdRequest
                    {
                        To = userId,
                        Image = new MediaImage
                        {
                            Id = dispatch.ChatUpdate.MediaHandle,
                            Caption = message
                        }
                    };

                    await _whatsAppClient.SendImageAttachmentMessageByIdAsync(imageMessageRequest);

                    _logger.LogInformation("✅ Sent WhatsApp image to {UserId} with media_id: {MediaId}", 
                        userId, dispatch.ChatUpdate.MediaHandle);
                }
                else
                {
                    // Send regular text message
                    if (string.IsNullOrWhiteSpace(message))
                    {
                        _logger.LogWarning("Skipping WhatsApp dispatch due to missing message content.");
                        return;
                    }

                    var textMessageRequest = new TextMessageRequest
                    {
                        To = userId,
                        Text = new WhatsAppText
                        {
                            Body = message,
                            PreviewUrl = false
                        }
                    };

                    var jsonPayload = JsonSerializer.Serialize(textMessageRequest, new JsonSerializerOptions { WriteIndented = true });
                    _logger.LogInformation("Sending WhatsApp message payload: {Payload}", jsonPayload);

                    await _whatsAppClient.SendTextMessageAsync(textMessageRequest);

                    _logger.LogInformation("✅ Sent WhatsApp text message to {UserId}", userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to send WhatsApp message to {UserId}", userId);
            }
        }
    }
}
