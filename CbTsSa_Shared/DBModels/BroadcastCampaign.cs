using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CbTsSa_Shared.DBModels
{
    /// <summary>
    /// Represents a broadcast campaign using an approved template.
    /// </summary>
    public class BroadcastCampaign
    {
        [Key]
        public int CampaignId { get; set; }

        [Required]
        public int TemplateId { get; set; }

        [Required]
        public long BusinessId { get; set; }

        [Required]
        [MaxLength(450)]
        public string InitiatedByUserId { get; set; }

        [Required]
        public int TotalRecipients { get; set; }

        public int SentCount { get; set; }

        public int DeliveredCount { get; set; }

        public int ReadCount { get; set; }

        public int FailedCount { get; set; }

        /// <summary>
        /// Status: Queued, Sending, Completed, Failed
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Status { get; set; }

        [Required]
        public DateTime CreatedDateTime { get; set; }

        public DateTime? CompletedDateTime { get; set; }

        // Navigation properties
        [ForeignKey(nameof(TemplateId))]
        public virtual BroadcastTemplate Template { get; set; }

        [ForeignKey(nameof(BusinessId))]
        public virtual Business Business { get; set; }

        [ForeignKey(nameof(InitiatedByUserId))]
        public virtual ApplicationUser InitiatedByUser { get; set; }

        public virtual ICollection<BroadcastMessage> Messages { get; set; }
    }
}