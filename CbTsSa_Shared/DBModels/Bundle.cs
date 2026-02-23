namespace CbTsSa_Shared.DBModels
{
    public class Bundle
    {
        public long BundleID { get; set; }
        public long ItemID { get; set; }
        
        // Navigation properties
        public Item Item { get; set; } = null!;
    }
}