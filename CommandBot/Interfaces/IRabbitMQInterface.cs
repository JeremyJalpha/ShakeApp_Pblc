using CbTsSa_Shared.Models;
using Microsoft.Extensions.Options;

namespace CommandBot.Interfaces
{
    public interface IRabbitMQInterface
    {
        Task InitializeAsync(IOptions<RabbitMqSettings> options);

        // Start consuming command queue and pass the worker CancellationToken to handlers.
        Task StartConsumingAsync(Func<string, CancellationToken, Task> processCommand, CancellationToken cancellationToken);

        // Outbound queues also accept handler with CancellationToken
        Task StartConsumingTelegramAsync(Func<string, CancellationToken, Task> handleTelegramDispatch, CancellationToken cancellationToken);
        Task StartConsumingWhatsAppAsync(Func<string, CancellationToken, Task> handleWhatsAppDispatch, CancellationToken cancellationToken);
        Task StartConsumingImageProcessingAsync(Func<string, CancellationToken, Task> handleImageProcessing, CancellationToken cancellationToken);

        void PublishTelegramOutbound(string json);
        void PublishWhatsAppOutbound(string json);
        void PublishCommand(string commandJson);
        void PublishOutboundMessage(string outboundJson);
        void PublishImageProcessingJob(string json);
    }
}