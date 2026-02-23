namespace CbTsSa_Shared.DBModels
{
    public class Purchasable
    {
        public long PurchasableID { get; set; }
        public long SaleableID { get; set; }
        public long OfferID { get; set; }
        
        // Navigation properties
        public Saleable Saleable { get; set; } = null!;
        public Offer Offer { get; set; } = null!;
    }
}