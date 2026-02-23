namespace CbTsSa_Shared.DBModels
{
    public class Good
    {
        public long GoodID { get; set; }
        public long? Barcode { get; set; }
        public long? ManufacturerID { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public bool IsPrivate { get; set; }
        public byte[]? Image { get; set; }
    }
}