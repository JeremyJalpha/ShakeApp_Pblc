namespace CbTsSa_Shared.DBModels
{
    public class DeliveryLeg
    {
        public long DeliveryLegID { get; set; }
        public long SaleID { get; set; }
        public DateTime? DtTmDriverOnScene { get; set; }
        public DateTime? DtTmCollected { get; set; }
        public DateTime? DtTmArrivedAtCust { get; set; }
        public DateTime? DtTmOTPPassed { get; set; }
        public DateTime? DtTmPaid { get; set; }
        public DateTime? DtTmDisputed { get; set; }
        public string? DisputedReason { get; set; }
        public DateTime? DtTmResolved { get; set; }
        public string? Resolution { get; set; }
        
        // Navigation properties
        public Sale Sale { get; set; } = null!;
        public List<DeliveryDriverLeg>? DeliveryDriverLegs { get; set; }
    }
}