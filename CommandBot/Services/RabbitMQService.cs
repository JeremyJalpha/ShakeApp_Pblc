using CbTsSa_Shared.CbTsSaConstants;
using CbTsSa_Shared.Models;
using CommandBot.Interfaces;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace CommandBot.Services
{
    public class RabbitMQService : IRabbitMQInterface
    {
        private IConnection _connection;
        private IChannel _channel;
        private readonly TaskCompletionSource<bool> _initializationComplete = new();
        private readonly ILogger<RabbitMQService> _logger;

        public RabbitMQService(ILogger<RabbitMQService> logger)
        {
            _channel = null!;
            _connection = null!;
            _logger = logger;
        }

        public async Task InitializeAsync(IOptions<RabbitMqSettings> options)
        {
            var settings = options.Value;
            ConnectionFactory factory;

            if (!string.IsNullOrWhiteSpace(settings.ConnectionString))
            {
                // CloudAMQP mode
                _logger.LogInformation("RabbitMQService: Using CloudAMQP connection string");

                factory = new ConnectionFactory
                {
                    Uri = new Uri(settings.ConnectionString),
                    AutomaticRecoveryEnabled = true,
                    TopologyRecoveryEnabled = true
                };
            }
            else
            {
                // Local Docker mode
                _logger.LogInformation("RabbitMQService: Using local RabbitMQ at {Host}:{Port}", settings.Host, settings.Port);

                factory = new ConnectionFactory
                {
                    HostName = settings.Host,
                    UserName = settings.Username,
                    Password = settings.Password,
                    Port = settings.Port > 0 ? settings.Port : AmqpTcpEndpoint.UseDefaultPort,
                    AutomaticRecoveryEnabled = true,
                    TopologyRecoveryEnabled = true,
                    RequestedConnectionTimeout = TimeSpan.FromSeconds(30),
                    SocketReadTimeout = TimeSpan.FromSeconds(30),
                    SocketWriteTimeout = TimeSpan.FromSeconds(30)
                };
            }

            // Retry logic
            const int maxRetries = 10;
            const int retryDelayMs = 3000;

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    _logger.LogInformation("Connection attempt {Attempt} of {MaxRetries}", i + 1, maxRetries);

                    _connection = await factory.CreateConnectionAsync();
                    _channel = await _connection.CreateChannelAsync();

                    _logger.LogInformation("Successfully connected to RabbitMQ");
                    break;
                }
                catch (Exception ex) when (i < maxRetries - 1)
                {
                    _logger.LogWarning(ex, "Failed to connect to RabbitMQ (attempt {Attempt}). Retrying in {Delay}ms...",
                        i + 1, retryDelayMs);
                    await Task.Delay(retryDelayMs);
                }
                catch (Exception ex) when (i == maxRetries - 1)
                {
                    _logger.LogError(ex, "Failed to connect to RabbitMQ after {MaxRetries} attempts", maxRetries);
                    throw;
                }
            }

            // Declare queues
            await _channel.QueueDeclareAsync(queue: CbTsSaConstants.CommandQueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            await _channel.QueueDeclareAsync(queue: CbTsSaConstants.TelegramOutboundQueue, durable: true, exclusive: false, autoDelete: false, arguments: null);
            await _channel.QueueDeclareAsync(queue: CbTsSaConstants.WhatsAppOutboundQueue, durable: true, exclusive: false, autoDelete: false, arguments: null);
            await _channel.QueueDeclareAsync(queue: CbTsSaConstants.ImageProcessingQueue, durable: true, exclusive: false, autoDelete: false, arguments: null);

            _logger.LogInformation("RabbitMQ queues declared successfully");

            _initializationComplete.SetResult(true);
        }

        private async Task WaitForInitializationAsync()
        {
            await _initializationComplete.Task;
        }

        public async void PublishCommand(string commandJson)
        {
            var body = Encoding.UTF8.GetBytes(commandJson);
            var props = new RabbitMQ.Client.BasicProperties();

            await _channel.BasicPublishAsync(
                exchange: "",
                routingKey: CbTsSaConstants.CommandQueueName,
                mandatory: false,
                basicProperties: props,
                body: body
            );
        }

        public async void PublishTelegramOutbound(string json)
        {
            var props = new RabbitMQ.Client.BasicProperties();
            await _channel.BasicPublishAsync(
                exchange: "",
                routingKey: CbTsSaConstants.TelegramOutboundQueue,
                mandatory: false,
                basicProperties: props,
                body: Encoding.UTF8.GetBytes(json)
            );
        }

        public async void PublishWhatsAppOutbound(string json)
        {
            var props = new RabbitMQ.Client.BasicProperties();
            await _channel.BasicPublishAsync(
                exchange: "",
                routingKey: CbTsSaConstants.WhatsAppOutboundQueue,
                mandatory: false,
                basicProperties: props,
                body: Encoding.UTF8.GetBytes(json)
            );
        }

        public async void PublishOutboundMessage(string outboundJson)
        {
            var body = Encoding.UTF8.GetBytes(outboundJson);
            var props = new RabbitMQ.Client.BasicProperties();

            await _channel.BasicPublishAsync(
                exchange: "",
                routingKey: CbTsSaConstants.TelegramOutboundQueue,
                mandatory: false,
                basicProperties: props,
                body: body
            );
        }

        // Updated: accept handler that receives CancellationToken and pass it through
        public async Task StartConsumingAsync(Func<string, CancellationToken, Task> processCommand, CancellationToken cancellationToken)
        {
            await WaitForInitializationAsync();

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    // forward the worker cancellation token to the handler
                    await processCommand(message, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message");
                }
            };

            await _channel.BasicConsumeAsync(queue: CbTsSaConstants.CommandQueueName, autoAck: true, consumer: consumer);
        }

        public async Task StartConsumingWhatsAppAsync(Func<string, CancellationToken, Task> handleWhatsAppDispatch, CancellationToken cancellationToken)
        {
            await WaitForInitializationAsync();

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    await handleWhatsAppDispatch(json, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "WhatsApp dispatch error");
                }
            };

            await _channel.BasicConsumeAsync(
                queue: CbTsSaConstants.WhatsAppOutboundQueue,
                autoAck: true,
                consumer: consumer
            );
        }

        public async Task StartConsumingTelegramAsync(Func<string, CancellationToken, Task> handleTelegramDispatch, CancellationToken cancellationToken)
        {
            await WaitForInitializationAsync();

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    await handleTelegramDispatch(json, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Telegram dispatch error");
                }
            };

            await _channel.BasicConsumeAsync(
                queue: CbTsSaConstants.TelegramOutboundQueue,
                autoAck: true,
                consumer: consumer
            );
        }

        public async void PublishImageProcessingJob(string json)
        {
            await WaitForInitializationAsync();

            var props = new RabbitMQ.Client.BasicProperties();
            await _channel.BasicPublishAsync(
                exchange: "",
                routingKey: CbTsSaConstants.ImageProcessingQueue,
                mandatory: false,
                basicProperties: props,
                body: Encoding.UTF8.GetBytes(json)
            );

            _logger.LogInformation("Published image processing job to queue");
        }

        public async Task StartConsumingImageProcessingAsync(Func<string, CancellationToken, Task> handleImageProcessing, CancellationToken cancellationToken)
        {
            await WaitForInitializationAsync();

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    await handleImageProcessing(json, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Image processing error");
                }
            };

            await _channel.BasicConsumeAsync(
                queue: CbTsSaConstants.ImageProcessingQueue,
                autoAck: true,
                consumer: consumer
            );
        }
    }
}