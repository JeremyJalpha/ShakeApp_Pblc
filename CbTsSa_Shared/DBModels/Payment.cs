namespace CbTsSa_Shared.DBModels
{
    public class Payment
    {
        public long PaymentID { get; set; }
        public long? SaleID { get; set; }
        public string? SenderEmail { get; set; }
        public long? CardID { get; set; }
        public decimal? Amount { get; set; }
        public string? CurrencyCode { get; set; }
        public byte? StatusID { get; set; }
        public DateTime InitiationDateTime { get; set; }
        public int? PayGateTransactionID { get; set; }
        public string? PayGateBankAuthID { get; set; }
        public DateTime? SuccesfulDateTime { get; set; }
        
        // Navigation properties
        public Sale? Sale { get; set; }
        public List<Failed>? Failures { get; set; }
        public List<Refund>? Refunds { get; set; }
    }
}