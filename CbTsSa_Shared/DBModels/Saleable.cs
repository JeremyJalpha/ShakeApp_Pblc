namespace CbTsSa_Shared.DBModels
{
    public class Saleable
    {
        public long SaleableID { get; set; }
        public long? ServiceID { get; set; }
        public long? ProductID { get; set; }
        public bool IsService { get; set; }
        
        // Navigation properties
        public Service? Service { get; set; }
        public Product? Product { get; set; }
        public List<Comment>? Comments { get; set; }
    }
}