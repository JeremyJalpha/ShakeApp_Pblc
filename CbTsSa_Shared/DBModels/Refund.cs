namespace CbTsSa_Shared.DBModels
{
    public class Refund
    {
        public long RefundID { get; set; }
        public long? PaymentID { get; set; }
        public DateTime InitiationDateTime { get; set; }
        public int? PayGateTransactionID { get; set; }
        public string? PayGateBankAuthID { get; set; }
        public DateTime? SuccesfulDateTime { get; set; }
        public string? Reason { get; set; }
        
        // Navigation properties
        public Payment? Payment { get; set; }
    }
}