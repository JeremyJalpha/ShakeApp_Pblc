using CommandBot.Interfaces;
using CbTsSa_Shared.Interfaces;
using CbTsSa_Shared.CbTsSaConstants;
using CbTsSa_Shared.Models;
using System.Text.Json;

namespace CommandBot.Services
{
    public class TelegramConsumerService : BackgroundService
    {
        private readonly ILogger<TelegramConsumerService> _logger;
        private readonly IRabbitMQInterface _rabbit;
        private readonly ITelegramDispatchService _dispatch;

        public TelegramConsumerService(
            ILogger<TelegramConsumerService> logger,
            IRabbitMQInterface rabbit,
            ITelegramDispatchService dispatch)
        {
            _logger = logger;
            _rabbit = rabbit;
            _dispatch = dispatch;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _rabbit.StartConsumingTelegramAsync(
                async (json, token) => await HandleTelegramMessage(json), 
                stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
                await Task.Delay(1000, stoppingToken);
        }

        private async Task HandleTelegramMessage(string json)
        {
            try
            {
                var request = JsonSerializer.Deserialize<ChatDispatchRequest>(json);

                if (request == null || !IsValidTelegramPayload(request))
                {
                    _logger.LogWarning("Telegram ChatUpdate.From.CellNumber is null or missing.");
                    return;
                }

                await _dispatch.DispatchAsync(request);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse Telegram message JSON: {Json}", json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dispatching Telegram message: {Json}", json);
            }
        }

        private static bool IsValidTelegramPayload(ChatDispatchRequest? payload)
        {
            if (payload?.ChatUpdate?.Channel != ChatChannelType.Telegram)
                return false;

            if (string.IsNullOrWhiteSpace(payload.ChatUpdate.Body))
                return false;

            var cellNumber = payload.ChatUpdate.From?.CellNumber;
            
            // Reject null, empty, whitespace, or default "-1" value
            if (string.IsNullOrWhiteSpace(cellNumber) || cellNumber == "-1")
                return false;

            return true;
        }
    }
}