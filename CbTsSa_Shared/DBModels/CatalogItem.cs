using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CbTsSa_Shared.DBModels
{
    public class CatalogItem
    {
        [Key]
        [Column(Order = 1)]
        [ForeignKey("Catalog")]
        public long CatalogID { get; set; }

        [Key]
        [Column(Order = 2)]
        [ForeignKey("Item")]
        public long ItemID { get; set; }

        /// <summary>
        /// Human-friendly menu code for ordering (e.g., "M_024", "D_003").
        /// Must be unique within a catalog.
        /// </summary>
        [Required]
        [MaxLength(10)]
        public string MenuCode { get; set; } = string.Empty;

        // Navigation properties
        public Catalog Catalog { get; set; } = null!;
        public Item Item { get; set; } = null!;
    }
}
