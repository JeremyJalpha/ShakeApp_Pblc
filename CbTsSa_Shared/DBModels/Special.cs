namespace CbTsSa_Shared.DBModels
{
    public class Special
    {
        public long SpecialID { get; set; }
        public required string CreatedByUserID { get; set; }
        public required string SpecialName { get; set; }
        public required string SpecialDescription { get; set; }
        public DateTime SpecialDtTmStart { get; set; }
        public DateTime SpecialDtTmEnd { get; set; }
        public decimal? Discount { get; set; }
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public ApplicationUser CreatedByUser { get; set; } = null!;
    }
}