namespace CbTsSa_Shared.DBModels
{
    public class Status
    {
        public byte StatusID { get; set; }
        public required string StatusName { get; set; }
        public required string StatusDescription { get; set; }
        
        // Navigation properties
        public List<SaleStatus>? SaleStatuses { get; set; }
    }
}