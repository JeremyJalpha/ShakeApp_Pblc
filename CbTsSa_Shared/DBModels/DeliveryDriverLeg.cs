namespace CbTsSa_Shared.DBModels
{
    public class DeliveryDriverLeg
    {
        public long DeliveryDriverLegID { get; set; }
        public long DeliveryLegID { get; set; }
        public long SaleID { get; set; }
        public long DeliveryID { get; set; }
        public required string DriverID { get; set; }
        
        // Navigation properties
        public DeliveryLeg DeliveryLeg { get; set; } = null!;
        public Delivery Delivery { get; set; } = null!;
        public Driver Driver { get; set; } = null!;
    }
}