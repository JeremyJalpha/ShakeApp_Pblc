namespace CbTsSa_Shared.DBModels
{
    public class OfferType
    {
        public byte OfferTypeID { get; set; }
        public required string TypeName { get; set; }
        public string? Description { get; set; }
    }
}