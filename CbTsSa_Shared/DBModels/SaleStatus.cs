namespace CbTsSa_Shared.DBModels
{
    public class SaleStatus
    {
        public long SaleID { get; set; }
        public long ChangedByUserID { get; set; }
        public DateTime DtTmStatusChanged { get; set; }
        public byte NewStatusID { get; set; }
        public string? ChangeReason { get; set; }
        
        // Navigation properties
        public Sale Sale { get; set; } = null!;
        public Status Status { get; set; } = null!;
    }
}