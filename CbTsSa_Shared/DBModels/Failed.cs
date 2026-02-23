namespace CbTsSa_Shared.DBModels
{
    public class Failed
    {
        public long FailedID { get; set; }
        public long? PaymentID { get; set; }
        public bool IsRefund { get; set; }
        public DateTime FailureDateTime { get; set; }
        public int? PayGateTransactionID { get; set; }
        public string? PayGateBankAuthID { get; set; }
        public byte? PayGateTransactionCode { get; set; }
        public int? PayGateResultCode { get; set; }
        public int? PayGateValidationErrorCode { get; set; }
        public int MopaErrorCode { get; set; }
        
        // Navigation properties
        public Payment? Payment { get; set; }
    }
}