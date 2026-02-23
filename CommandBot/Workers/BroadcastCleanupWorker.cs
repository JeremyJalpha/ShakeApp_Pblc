using CbTsSa_Shared.CbTsSaConstants;
using CbTsSa_Shared.DBModels;
using Microsoft.EntityFrameworkCore;

namespace CommandBot.Workers
{
    /// <summary>
    /// Background worker that cleans up old broadcast data to prevent database bloat.
    /// Runs once per day at 2 AM.
    /// </summary>
    public class BroadcastCleanupWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BroadcastCleanupWorker> _logger;

        public BroadcastCleanupWorker(IServiceProvider serviceProvider, ILogger<BroadcastCleanupWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BroadcastCleanupWorker started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Wait until 2 AM
                    await WaitUntilNextCleanupTimeAsync(stoppingToken);
                    
                    if (!stoppingToken.IsCancellationRequested)
                    {
                        await PerformCleanupAsync(stoppingToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Expected when stopping
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during broadcast cleanup");
                    // Wait a bit before retrying to avoid rapid failure loops
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }

            _logger.LogInformation("BroadcastCleanupWorker stopped");
        }

        private async Task WaitUntilNextCleanupTimeAsync(CancellationToken cancellationToken)
        {
            var now = DateTime.Now;
            var nextCleanup = now.Date.AddHours(2); // 2 AM today
            
            if (now.Hour >= 2)
                nextCleanup = nextCleanup.AddDays(1); // Already passed, schedule for tomorrow
            
            var delay = nextCleanup - now;
            _logger.LogInformation("Next cleanup scheduled for {Time} (in {Hours:F1} hours)", 
                nextCleanup, delay.TotalHours);
            
            await Task.Delay(delay, cancellationToken);
        }

        private async Task PerformCleanupAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            _logger.LogInformation("Starting broadcast data cleanup");

            // Delete old completed campaigns and their messages
            var campaignCutoff = DateTime.UtcNow - BroadcastLimits.CompletedCampaignRetentionPeriod;
            var oldCampaigns = await dbContext.BroadcastCampaigns
                .Where(c => c.Status == "Completed" && c.CompletedDateTime < campaignCutoff)
                .ToListAsync(cancellationToken);

            if (oldCampaigns.Any())
            {
                dbContext.BroadcastCampaigns.RemoveRange(oldCampaigns);
                _logger.LogInformation("Deleted {Count} old completed campaigns", oldCampaigns.Count);
            }

            // Mark old inactive images as inactive (soft delete)
            var imageCutoff = DateTime.UtcNow - BroadcastLimits.ImageRetentionPeriod;
            var oldImages = await dbContext.CampaignImages
                .Where(ci => ci.IsActive && ci.UploadedDateTime < imageCutoff)
                .ToListAsync(cancellationToken);

            foreach (var image in oldImages)
            {
                image.IsActive = false;
            }

            if (oldImages.Any())
            {
                _logger.LogInformation("Deactivated {Count} old campaign images", oldImages.Count);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Broadcast data cleanup completed");
        }
    }
}