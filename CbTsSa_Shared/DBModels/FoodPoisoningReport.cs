using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CbTsSa_Shared.DBModels
{
    public class FoodPoisoningReport
    {
        [Key]
        public int ReportId { get; set; }

        [Required]
        public required string UserId { get; set; }

        [Required]
        [MaxLength(500)]
        public required string IncidentAddress { get; set; }

        [Required]
        [MaxLength(200)]
        public required string FoodItem { get; set; }

        [Required]
        public int PeopleAffected { get; set; }

        [Required]
        [MaxLength(50)]
        public required string SeverityLevel { get; set; }

        public DateTime DateTimeReported { get; set; }


        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }
    }
}