namespace CbTsSa_Shared.Models
{
    /// <summary>
    /// Background job for processing user ID images.
    /// Queued via RabbitMQ for async download and S3 upload.
    /// </summary>
    public class ImageProcessingJob
    {
        public int UserIdImageId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string MediaHandle { get; set; } = string.Empty;
        public string ImageType { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
        public DateTime QueuedDateTime { get; set; } = DateTime.UtcNow;
    }
}
