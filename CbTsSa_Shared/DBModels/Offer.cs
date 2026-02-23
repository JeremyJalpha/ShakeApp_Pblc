namespace CbTsSa_Shared.DBModels
{
    public class Offer
    {
        public long OfferID { get; set; }
        public byte OfferTypeID { get; set; }
        public required string CreatedByUserID { get; set; }
        public decimal? BasePrice { get; set; }
        public DateTime OfferDtTmStart { get; set; }
        public DateTime OfferDtTmEnd { get; set; }
        
        // Navigation properties
        public OfferType OfferType { get; set; } = null!;
        public ApplicationUser CreatedByUser { get; set; } = null!;
    }
}