namespace CbTsSa_Shared.DBModels
{
    public class Item
    {
        public long ItemID { get; set; }
        public long PurchasableID { get; set; }
        public long? SpecialID { get; set; }
        
        // Navigation properties
        public Purchasable Purchasable { get; set; } = null!;
        public Special? Special { get; set; }
        public List<CatalogItem>? CatalogItems { get; set; }
    }
}