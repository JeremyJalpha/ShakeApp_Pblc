namespace CbTsSa_Shared.DBModels
{
    public class SaleBasket
    {
        public long SaleID { get; set; }
        public long EffectiveBasketID { get; set; }
        
        // Navigation properties
        public Sale Sale { get; set; } = null!;
        public EffectiveBasket EffectiveBasket { get; set; } = null!;
    }
}