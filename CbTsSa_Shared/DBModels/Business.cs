using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CbTsSa_Shared.DBModels
{
    public class Business
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long BusinessID { get; set; }

        [Required]
        public long Cellnumber { get; set; }

        [StringLength(25)]
        public string BusinessName { get; set; } = string.Empty;

        [StringLength(25)]
        public string? PricelistPreamble { get; set; }

        [StringLength(25)]
        public string? NewUserGreeting_Cold { get; set; }

        [StringLength(25)]
        public string? NewUserGreeting_Warm { get; set; }

        [StringLength(25)]
        public string? RegisteredName { get; set; }

        public byte Industry { get; set; }

        [StringLength(25)]
        public string? TradingName { get; set; }

        public long? VATNumber { get; set; }

        [StringLength(25)]
        public string? StreetAddress { get; set; }

        public bool? PostalSameAsStreet { get; set; }

        [StringLength(25)]
        public string? PostalAddress { get; set; }

        [StringLength(25)]
        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(25)]
        [Url]
        public string? Website { get; set; }

        [StringLength(25)]
        public string? Facebook { get; set; }

        [StringLength(25)]
        public string? Twitter { get; set; }

        [StringLength(25)]
        public string? TradingHours { get; set; }

        public byte? Bank { get; set; }

        public int? BranchNumber { get; set; }

        public long? AccountNumber { get; set; }

        public byte? AccountType { get; set; }

        public decimal? GPSLat { get; set; }

        public decimal? GPSLong { get; set; }

        public DateTime? LocationLastUpdated { get; set; }

        public byte? AverageRating { get; set; }

        // Navigation properties
        public List<Catalog>? Catalogs { get; set; }
        public List<Service>? Services { get; set; }
        public List<Product>? Products { get; set; }
        public List<SignedUpWith>? UsersSignedUp { get; set; }
    }
}