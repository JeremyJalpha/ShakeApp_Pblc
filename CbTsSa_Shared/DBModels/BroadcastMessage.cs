using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CbTsSa_Shared.DBModels
{
    /// <summary>
    /// Represents an individual message in a broadcast campaign.
    /// </summary>
    public class BroadcastMessage
    {
        [Key]
        public int MessageId { get; set; }

        [Required]
        public int CampaignId { get; set; }

        [Required]
        [MaxLength(450)]
        public string RecipientUserId { get; set; }

        [Required]
        [MaxLength(20)]
        public string RecipientPhoneNumber { get; set; }

        /// <summary>
        /// WhatsApp message ID (for tracking)
        /// </summary>
        [MaxLength(100)]
        public string WhatsAppMessageId { get; set; }

        /// <summary>
        /// Status: Queued, Sent, Delivered, Read, Failed
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Status { get; set; }

        [Required]
        public DateTime CreatedDateTime { get; set; }

        public DateTime? SentDateTime { get; set; }

        public DateTime? DeliveredDateTime { get; set; }

        public DateTime? ReadDateTime { get; set; }

        [MaxLength(500)]
        public string ErrorMessage { get; set; }

        // Navigation properties
        [ForeignKey(nameof(CampaignId))]
        public virtual BroadcastCampaign Campaign { get; set; }

        [ForeignKey(nameof(RecipientUserId))]
        public virtual ApplicationUser Recipient { get; set; }
    }
}