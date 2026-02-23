using CommandBot.Interfaces;
using CbTsSa_Shared.Interfaces;
using CbTsSa_Shared.CbTsSaConstants;
using CbTsSa_Shared.Models;
using System.Text.Json;

namespace CommandBot.Services
{
    public class WhatsAppConsumerService : BackgroundService
    {
        private readonly ILogger<WhatsAppConsumerService> _logger;
        private readonly IRabbitMQInterface _rabbit;
        private readonly IWhatsAppDispatchService _whatsApp;

        public WhatsAppConsumerService(
            ILogger<WhatsAppConsumerService> logger,
            IRabbitMQInterface rabbit,
            IWhatsAppDispatchService whatsApp)
        {
            _logger = logger;
            _rabbit = rabbit;
            _whatsApp = whatsApp;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _rabbit.StartConsumingWhatsAppAsync(async (json, token) => await HandleWhatsAppMessage(json), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
                await Task.Delay(1000, stoppingToken);
        }

        private async Task HandleWhatsAppMessage(string json)
        {
            var payload = JsonSerializer.Deserialize<ChatDispatchRequest>(json);
            if (payload?.ChatUpdate?.Channel != ChatChannelType.WhatsApp || string.IsNullOrWhiteSpace(payload.ChatUpdate.Body))
                return;

            await _whatsApp.DispatchAsync(payload);
        }
    }
}