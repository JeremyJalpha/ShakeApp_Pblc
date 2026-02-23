using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CbTsSa_Shared.DBModels
{
    /// <summary>
    /// Stores media handles from WhatsApp/Telegram messages for use in broadcast campaigns.
    /// Users upload images via chat, and the system stores the platform-specific media handle.
    /// </summary>
    [Index(nameof(BusinessId), nameof(MediaHandle), IsUnique = true, Name = "IX_CampaignImage_Business_MediaHandle")]
    [Index(nameof(BusinessId), nameof(UploadedByUserId), nameof(IsActive), Name = "IX_CampaignImage_Business_User_Active")]
    public class CampaignImage
    {
        [Key]
        public int CampaignImageId { get; set; }

        [Required]
        public long BusinessId { get; set; }

        /// <summary>
        /// The ID of the user who uploaded this image (ApplicationUser.Id).
        /// </summary>
        [Required]
        [MaxLength(450)]
        public string UploadedByUserId { get; set; }

        /// <summary>
        /// Platform-specific media handle (WhatsApp media ID or Telegram file_id).
        /// Max length increased to accommodate resumable upload handles (upload:... or 4::...).
        /// </summary>
        [Required]
        [MaxLength(255)]  // ✅ Increased from 25
        public string MediaHandle { get; set; }

        /// <summary>
        /// Business-owned media handle (uploaded by the business / phone_number) suitable for use in
        /// WhatsApp template headers. This is populated by re-uploading a user's inbound media
        /// to the business phone_number via the /{phone_number_id}/media endpoint.
        /// Nullable: if re-upload failed we will keep the original MediaHandle.
        /// </summary>
        [MaxLength(255)]  // ✅ Added constraint
        public string? BusinessMediaHandle { get; set; }

        /// <summary>
        /// Optional caption/description for the image (not used in simplified workflow)
        /// </summary>
        [MaxLength(200)]
        public string? Caption { get; set; }

        /// <summary>
        /// Platform where image was uploaded (WhatsApp, Telegram, etc.)
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Platform { get; set; }

        [Required]
        public DateTime UploadedDateTime { get; set; }

        [Required]
        public bool IsActive { get; set; }

        /// <summary>
        /// Optional: URL if media was downloaded and stored locally
        /// </summary>
        [MaxLength(500)]  // ✅ Increased for full URLs
        public string? LocalStorageUrl { get; set; }

        // Navigation properties
        [ForeignKey(nameof(BusinessId))]
        public virtual Business Business { get; set; }

        [ForeignKey(nameof(UploadedByUserId))]
        public virtual ApplicationUser UploadedByUser { get; set; }

        /// <summary>
        /// Returns the effective media handle to use (prefers business handle if available).
        /// </summary>
        [NotMapped]
        public string EffectiveMediaHandle =>
            !string.IsNullOrWhiteSpace(BusinessMediaHandle)
                ? BusinessMediaHandle
                : MediaHandle;
    }
}