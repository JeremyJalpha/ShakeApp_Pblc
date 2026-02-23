using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CbTsSa_Shared.DBModels
{
    /// <summary>
    /// Stores user ID images (front and back).
    /// Enforces strict rules: maximum 2 images per user (one front, one back).
    /// 
    /// Images are stored permanently in cloud storage (S3, Azure, etc).
    /// This table only stores metadata and references.
    /// </summary>
    public class UserIdImage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string ImageType { get; set; } = string.Empty; // "front" or "back"

        /// <summary>
        /// Temporary media handle from WhatsApp/Telegram (expires in 30 days).
        /// Used by background worker to download the image.
        /// </summary>
        public string? MediaHandle { get; set; }

        /// <summary>
        /// S3 key or Azure blob path where the image is permanently stored.
        /// E.g., "users/{userId}/idimages/front/20240115_143022_passport.jpg"
        /// Null until background processing completes.
        /// </summary>
        public string? StoragePath { get; set; }

        /// <summary>
        /// Storage provider name: "S3", "Azure", "Local", etc.
        /// </summary>
        [StringLength(20)]
        public string? StorageType { get; set; }

        /// <summary>
        /// Processing status: "Pending", "Processing", "Completed", "Failed"
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// Error message if processing failed
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Platform where image came from: "WhatsApp" or "Telegram"
        /// </summary>
        public string? Platform { get; set; }

        /// <summary>
        /// Original filename from the upload
        /// </summary>
        public string? OriginalFilename { get; set; }

        /// <summary>
        /// File size in bytes (0 until processing completes)
        /// </summary>
        public long FileSizeBytes { get; set; }

        public DateTime UploadedDateTime { get; set; } = DateTime.UtcNow;

        public DateTime? ProcessedDateTime { get; set; }

        // Navigation property
        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }
    }
}
