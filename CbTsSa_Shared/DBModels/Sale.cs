namespace CbTsSa_Shared.DBModels
{
    public class Sale
    {
        public long SaleID { get; set; }
        public required string UserID { get; set; }
        public bool DontPoolWithStrangers { get; set; }
        public DateTime DtTmRequestedCheckout { get; set; }
        public int? ItemCount { get; set; }
        public bool IsAmtManual { get; set; }
        public decimal? TotalAmount { get; set; }
        public DateTime? DtTmCompleted { get; set; }
        
        // Navigation properties
        public ApplicationUser User { get; set; } = null!;
        public List<SaleBasket>? SaleBaskets { get; set; }
        public List<DeliveryLeg>? DeliveryLegs { get; set; }
        public List<Payment>? Payments { get; set; }
        public List<SaleStatus>? SaleStatuses { get; set; }
    }
}