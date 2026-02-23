namespace CbTsSa_Shared.DBModels
{
    public class AdditionalDiscount
    {
        public long GatheredBasketID { get; set; }
        public required string AuthorisedByUserID { get; set; }
        public decimal Discount { get; set; }
        public required string DiscountReason { get; set; }
        
        // Navigation properties
        public GatheredBasket GatheredBasket { get; set; } = null!;
        public ApplicationUser AuthorisedByUser { get; set; } = null!;
    }
}