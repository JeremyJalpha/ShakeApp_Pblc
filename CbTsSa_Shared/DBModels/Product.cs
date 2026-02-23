namespace CbTsSa_Shared.DBModels
{
    public class Product
    {
        public long ProductID { get; set; }
        public long BusinessID { get; set; }
        public long GoodID { get; set; }
        
        // Navigation properties
        public Good Good { get; set; } = null!;
        public Business Business { get; set; } = null!;
    }
}