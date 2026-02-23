using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CbTsSa_Shared.DBModels
{
    public class UserSignUp
    {
        [Key]
        [Required]
        public required string UserId { get; set; }

        [Required]
        [MaxLength(200)]
        public required string Name { get; set; }

        [Required]
        [MaxLength(200)]
        public required string Email { get; set; }

        [Required]
        [MaxLength(500)]
        public required string Address { get; set; }

        [Required]
        [MaxLength(200)]
        public required string EmergencyContactName { get; set; }

        [Required]
        [MaxLength(50)]
        public required string EmergencyContactNumber { get; set; }

        [Required]
        [MaxLength(1000)]
        public required string Reason { get; set; }

        [MaxLength(200)]
        public string? FirstSignedUpWith { get; set; }

        public DateTime DateTimeSubmitted { get; set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }
    }
}
