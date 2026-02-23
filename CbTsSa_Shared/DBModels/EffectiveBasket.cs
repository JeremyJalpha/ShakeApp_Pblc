namespace CbTsSa_Shared.DBModels
{
    public class EffectiveBasket
    {
        public long EffectiveBasketID { get; set; }
        public long GatheredBasketID { get; set; }
        public decimal? EffectiveDiscount { get; set; }
        public string? AuthorisedByUserID { get; set; }
        
        // Navigation properties
        public GatheredBasket GatheredBasket { get; set; } = null!;
        public ApplicationUser? AuthorisedByUser { get; set; }
    }
}