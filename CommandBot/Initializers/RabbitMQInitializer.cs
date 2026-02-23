using CommandBot.Services;
using CbTsSa_Shared.Models;
using Microsoft.Extensions.Options;
using CommandBot.Interfaces;

namespace CommandBot.Workers
{
    public class RabbitMQInitializer : BackgroundService
    {
        private readonly IRabbitMQInterface _rabbitMQService;
        private readonly IOptions<RabbitMqSettings> _options;
        private readonly ILogger<RabbitMQInitializer> _logger;

        public RabbitMQInitializer(
            IRabbitMQInterface rabbitMQService,
            IOptions<RabbitMqSettings> options,
            ILogger<RabbitMQInitializer> logger)
        {
            _rabbitMQService = rabbitMQService;
            _options = options;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await _rabbitMQService.InitializeAsync(_options);
                _logger.LogInformation("RabbitMQ initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize RabbitMQ");
                throw;
            }
        }
    }
}