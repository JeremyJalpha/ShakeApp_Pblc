using CbTsSa_Shared.DBModels;
using CbTsSa_Shared.Models;
using CbTsSa_Shared.Services;
using CommandBot.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CommandBot.Workers
{
    /// <summary>
    /// Background worker that processes ID image uploads.
    /// Downloads media from WhatsApp/Telegram and uploads to S3/Azure permanently.
    /// </summary>
    public class ImageProcessingWorker : BackgroundService
    {
        private readonly ILogger<ImageProcessingWorker> _logger;
        private readonly IRabbitMQInterface _rabbit;
        private readonly IServiceScopeFactory _scopeFactory;

        public ImageProcessingWorker(
            ILogger<ImageProcessingWorker> logger,
            IRabbitMQInterface rabbit,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _rabbit = rabbit;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ImageProcessingWorker starting...");

            await _rabbit.StartConsumingImageProcessingAsync(ProcessImageJobAsync, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }

            _logger.LogInformation("ImageProcessingWorker stopped");
        }

        private async Task ProcessImageJobAsync(string json, CancellationToken cancellationToken)
        {
            ImageProcessingJob? job = null;

            try
            {
                job = JsonSerializer.Deserialize<ImageProcessingJob>(json);

                if (job == null)
                {
                    _logger.LogWarning("Failed to deserialize image processing job");
                    return;
                }

                _logger.LogInformation(
                    "Processing image job - ImageId: {ImageId}, UserId: {UserId}, Platform: {Platform}",
                    job.UserIdImageId,
                    job.UserId,
                    job.Platform);

                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var imageStorageService = scope.ServiceProvider.GetRequiredService<IImageStorageService>();

                // Get the pending image record
                var userIdImage = await dbContext.UserIdImages
                    .FirstOrDefaultAsync(uii => uii.Id == job.UserIdImageId, cancellationToken);

                if (userIdImage == null)
                {
                    _logger.LogWarning("UserIdImage not found - Id: {ImageId}", job.UserIdImageId);
                    return;
                }

                // Update status to Processing
                userIdImage.Status = "Processing";
                await dbContext.SaveChangesAsync(cancellationToken);

                // Download image from WhatsApp/Telegram using MediaHandle
                byte[]? imageBytes = null;

                switch (job.Platform.ToLower())
                {
                    case "whatsapp":
                        try
                        {
                            var whatsAppMediaService = scope.ServiceProvider.GetRequiredService<CommandBot.Services.WhatsAppMediaService>();

                            _logger.LogInformation(
                                "Downloading WhatsApp media - ImageId: {ImageId}, MediaHandle: {Handle}",
                                job.UserIdImageId,
                                job.MediaHandle);

                            imageBytes = await whatsAppMediaService.DownloadMediaAsync(
                                job.MediaHandle,
                                cancellationToken);

                            if (imageBytes != null && imageBytes.Length > 0)
                            {
                                _logger.LogInformation(
                                    "Successfully downloaded WhatsApp media - ImageId: {ImageId}, Size: {Size} bytes",
                                    job.UserIdImageId,
                                    imageBytes.Length);
                            }
                            else
                            {
                                _logger.LogWarning(
                                    "WhatsApp media download returned empty - ImageId: {ImageId}",
                                    job.UserIdImageId);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, 
                                "WhatsApp media download failed - ImageId: {ImageId}, MediaHandle: {Handle}",
                                job.UserIdImageId,
                                job.MediaHandle);
                        }
                        break;

                    case "telegram":
                        try
                        {
                            var telegramClient = scope.ServiceProvider.GetRequiredService<CommandBot.Clients.ITelegramClient>();

                            _logger.LogInformation(
                                "Downloading Telegram media - ImageId: {ImageId}, MediaHandle: {Handle}",
                                job.UserIdImageId,
                                job.MediaHandle);

                            imageBytes = await telegramClient.DownloadFileAsync(
                                job.MediaHandle,
                                cancellationToken);

                            if (imageBytes != null && imageBytes.Length > 0)
                            {
                                _logger.LogInformation(
                                    "Successfully downloaded Telegram media - ImageId: {ImageId}, Size: {Size} bytes",
                                    job.UserIdImageId,
                                    imageBytes.Length);
                            }
                            else
                            {
                                _logger.LogWarning(
                                    "Telegram media download returned empty - ImageId: {ImageId}",
                                    job.UserIdImageId);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex,
                                "Telegram media download failed - ImageId: {ImageId}, MediaHandle: {Handle}",
                                job.UserIdImageId,
                                job.MediaHandle);
                        }
                        break;

                    default:
                        _logger.LogWarning("Unknown platform: {Platform}", job.Platform);
                        break;
                }

                if (imageBytes == null || imageBytes.Length == 0)
                {
                    // Mark as failed
                    userIdImage.Status = "Failed";
                    userIdImage.ErrorMessage = $"Failed to download image from {job.Platform}";
                    await dbContext.SaveChangesAsync(cancellationToken);

                    _logger.LogError("Failed to download image - ImageId: {ImageId}, Platform: {Platform}",
                        job.UserIdImageId, job.Platform);
                    return;
                }

                // Upload to storage service
                var filename = $"id_{job.ImageType}_{DateTime.UtcNow:yyyyMMddHHmmss}.jpg";
                var storagePath = await imageStorageService.UploadImageAsync(
                    imageBytes,
                    job.UserId,
                    job.ImageType,
                    filename,
                    cancellationToken);

                // Update database with success
                userIdImage.StoragePath = storagePath;
                userIdImage.StorageType = imageStorageService.ProviderName;
                userIdImage.OriginalFilename = filename;
                userIdImage.FileSizeBytes = imageBytes.Length;
                userIdImage.Status = "Completed";
                userIdImage.ProcessedDateTime = DateTime.UtcNow;
                userIdImage.ErrorMessage = null;

                await dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Successfully processed image - ImageId: {ImageId}, StoragePath: {Path}, Size: {Size}",
                    job.UserIdImageId,
                    storagePath,
                    imageBytes.Length);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Image processing cancelled - ImageId: {ImageId}",
                    job?.UserIdImageId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing image job - ImageId: {ImageId}",
                    job?.UserIdImageId);

                // Try to mark as failed in database
                try
                {
                    if (job != null)
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                        var userIdImage = await dbContext.UserIdImages
                            .FirstOrDefaultAsync(uii => uii.Id == job.UserIdImageId, cancellationToken);

                        if (userIdImage != null)
                        {
                            userIdImage.Status = "Failed";
                            userIdImage.ErrorMessage = ex.Message;
                            await dbContext.SaveChangesAsync(cancellationToken);
                        }
                    }
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, "Failed to update error status in database");
                }
            }
        }
    }
}
