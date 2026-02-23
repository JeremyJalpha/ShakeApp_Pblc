using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CbTsSa_Shared.DBModels
{
    /// <summary>
    /// Represents a WhatsApp broadcast message template following Meta's Business Messaging guidelines.
    /// Templates must be pre-approved by WhatsApp before they can be used for broadcasting.
    /// </summary>
    public class BroadcastTemplate
    {
        [Key]
        public int TemplateId { get; set; }

        [Required]
        public long BusinessId { get; set; }

        [Required]
        [MaxLength(450)]
        public string CreatedByUserId { get; set; }

        /// <summary>
        /// Template name: lowercase, alphanumeric, and underscores only (max 512 chars)
        /// </summary>
        [Required]
        [MaxLength(512)]
        public string Name { get; set; }

        /// <summary>
        /// Category: MARKETING, UTILITY, or AUTHENTICATION
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Category { get; set; }

        /// <summary>
        /// Language code (e.g., en, en_US, en_GB, af, zu)
        /// </summary>
        [Required]
        [MaxLength(10)]
        public string Language { get; set; }

        /// <summary>
        /// Optional header text (max 60 characters)
        /// </summary>
        [MaxLength(60)]
        public string HeaderText { get; set; } = string.Empty;

        /// <summary>
        /// Required body text (max 1024 characters). Can include variables like {{1}}, {{2}}
        /// </summary>
        [Required]
        [MaxLength(1024)]
        public string BodyText { get; set; }

        /// <summary>
        /// Optional footer text (max 60 characters)
        /// </summary>
        [MaxLength(60)]
        public string FooterText { get; set; }

        /// <summary>
        /// Optional buttons in JSON format (max 3 buttons)
        /// </summary>
        [Column(TypeName = "text")]
        public string ButtonsJson { get; set; } = string.Empty;

        /// <summary>
        /// WhatsApp template ID (assigned after approval)
        /// </summary>
        [MaxLength(100)]
        public string WhatsAppTemplateId { get; set; } = string.Empty;

        /// <summary>
        /// Status: Pending, Approved, Rejected
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Status { get; set; }

        [Required]
        public bool IsActive { get; set; }

        [Required]
        public DateTime CreatedDateTime { get; set; }

        public DateTime? ApprovedDateTime { get; set; }

        /// <summary>
        /// Rejection reason from WhatsApp
        /// </summary>
        [MaxLength(500)]
        public string RejectionReason { get; set; } = string.Empty;

        /// <summary>
        /// Optional campaign image to include in the template header
        /// </summary>
        public int? CampaignImageId { get; set; }

        /// <summary>
        /// Header type: TEXT, IMAGE, VIDEO, DOCUMENT
        /// </summary>
        [MaxLength(20)]
        public string HeaderType { get; set; }

        // Navigation properties
        [ForeignKey(nameof(BusinessId))]
        public virtual Business Business { get; set; }

        [ForeignKey(nameof(CreatedByUserId))]
        public virtual ApplicationUser CreatedByUser { get; set; }

        [ForeignKey(nameof(CampaignImageId))]
        public virtual CampaignImage CampaignImage { get; set; }
    }
}