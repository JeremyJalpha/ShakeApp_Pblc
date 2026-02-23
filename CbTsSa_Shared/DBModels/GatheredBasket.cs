namespace CbTsSa_Shared.DBModels
{
    public class GatheredBasket
    {
        public long GatheredBasketID { get; set; }
        public long BundleID { get; set; }
        public int BundleMultiplier { get; set; }
        
        // Navigation properties
        public Bundle Bundle { get; set; } = null!;
        public List<AdditionalDiscount>? AdditionalDiscounts { get; set; }
    }
}