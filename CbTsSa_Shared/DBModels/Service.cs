namespace CbTsSa_Shared.DBModels
{
    using System.ComponentModel.DataAnnotations.Schema;

    public class Service
    {
        public long ServiceID { get; set; }
        public long BusinessID { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        
        // Navigation properties
        [ForeignKey(nameof(BusinessID))]
        public Business Business { get; set; } = null!;
    }
}
