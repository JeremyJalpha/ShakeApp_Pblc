using CommandBot.Interfaces;
using CbTsSa_Shared.CbTsSaConstants;
using System.Text.Json;

namespace CommandBot.Workers
{
    public class CommandWorker : BackgroundService
    {
        private readonly ILogger<CommandWorker> _logger;
        private readonly IRabbitMQInterface _rabbit;
        private readonly IServiceScopeFactory _scopeFactory;

        public CommandWorker(
            ILogger<CommandWorker> logger,
            IRabbitMQInterface rabbit,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _rabbit = rabbit;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            // pass the worker cancellation token to the rabbit consumer so message handlers can observe it
            await _rabbit.StartConsumingAsync(ProcessMessage, token);

            while (!token.IsCancellationRequested)
                await Task.Delay(1000, token);
        }

        // Now accepts CancellationToken propagated from the worker
        private async Task ProcessMessage(string json, CancellationToken ct)
        {
            // 🔍 ADD THIS LOGGING
            _logger.LogInformation("📥 CommandWorker received: {Json}", json);
            
            var envelope = JsonSerializer.Deserialize<Envelope>(json);
            var chatUpdate = envelope?.ChatUpdate;

            // 🔍 ADD THIS LOGGING
            _logger.LogInformation(
                "📥 Deserialized - MediaHandle: '{MediaHandle}', MessageType: {MessageType}, Body: {Body}",
                chatUpdate?.MediaHandle ?? "NULL",
                chatUpdate?.MessageType ?? ChatMessageType.Text,
                chatUpdate?.Body ?? "NULL"
            );

            if (chatUpdate is null || chatUpdate.From == null || string.IsNullOrWhiteSpace(chatUpdate.Body))
            {
                _logger.LogError("Bad payload: {json}", json);
                return;
            }

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var runner = scope.ServiceProvider.GetRequiredService<ICommandRunner>();

                // pass cancellation token into command execution pipeline
                var dispatches = await runner.ExecuteAsync(chatUpdate, ct);

                foreach (var dispatch in dispatches)
                {
                    var outboundJson = JsonSerializer.Serialize(dispatch);

                    switch (chatUpdate.Channel)
                    {
                        case ChatChannelType.Telegram:
                            _rabbit.PublishTelegramOutbound(outboundJson);
                            break;

                        case ChatChannelType.WhatsApp:
                            _rabbit.PublishWhatsAppOutbound(outboundJson);
                            break;

                        default:
                            _logger.LogWarning("Unknown channel: {Channel}", chatUpdate.Channel);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message: {json}", json);
            }
        }
    }
}