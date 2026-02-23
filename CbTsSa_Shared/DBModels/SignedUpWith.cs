using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CbTsSa_Shared.DBModels
{
    /// <summary>
    /// Junction table tracking which businesses a user has signed up through.
    /// </summary>
    public class SignedUpWith
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public long BusinessId { get; set; }

        public DateTime DateTimeSignedUp { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }

        [ForeignKey(nameof(BusinessId))]
        public Business? Business { get; set; }
    }
}
