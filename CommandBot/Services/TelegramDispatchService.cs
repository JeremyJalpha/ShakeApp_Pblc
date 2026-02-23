using CommandBot.Clients;
using CbTsSa_Shared.CbTsSaConstants;
using CbTsSa_Shared.Models;
using CbTsSa_Shared.Interfaces;

namespace CommandBot.Services
{
    public class TelegramDispatchService : ITelegramDispatchService
    {
        private readonly ITelegramClient _client;
        private readonly ILogger<TelegramDispatchService> _logger;

        public TelegramDispatchService(ITelegramClient client, ILogger<TelegramDispatchService> logger)
        {
            _client = client;
            _logger = logger;
        }
        // Keep the interface method for compatibility
        public Task DispatchAsync(ChatDispatchRequest dispatch)
            => DispatchAsync(dispatch, CancellationToken.None);

        // New overload accepting a CancellationToken for better testability and cancellation propagation
        public async Task DispatchAsync(ChatDispatchRequest dispatch, CancellationToken cancellationToken)
        {
            var chatId = long.Parse(dispatch.ChatUpdate.From.CellNumber);
            var text = dispatch.ChatUpdate.Body;

            try
            {
                // Check if this is an image message
                if (!string.IsNullOrWhiteSpace(dispatch.ChatUpdate.MediaHandle) && 
                    dispatch.ChatUpdate.MessageType == ChatMessageType.Image)
                {
                    await _client.SendPhotoByFileIdAsync(chatId, dispatch.ChatUpdate.MediaHandle, text, cancellationToken);
                    _logger.LogInformation("✅ Sent Telegram image to {ChatId} with file_id: {FileId}", chatId, dispatch.ChatUpdate.MediaHandle);
                }
                else
                {
                    await _client.SendMessageAsync(chatId, text, cancellationToken);
                    _logger.LogInformation("✅ Sent Telegram text message to {ChatId}", chatId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to send Telegram message to {ChatId}", chatId);
            }
        }
    }
}
