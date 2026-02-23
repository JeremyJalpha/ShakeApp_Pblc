namespace CbTsSa_Shared.DBModels
{
    public class Delivery
    {
        public long DeliveryID { get; set; }
        public DateTime DtTmCreated { get; set; }
        public DateTime? DtTmDriverAccepted { get; set; }
        public string? DriverAcceptedLocation { get; set; } // GPS coordinates as string
        public DateTime? DtTmDriverOnScene { get; set; }
        public DateTime? DtTmClosed { get; set; }
        
        // Navigation properties
        public List<DeliveryDriverLeg>? DeliveryDriverLegs { get; set; }
    }
}