namespace CbTsSa_Shared.DBModels
{
    public class Driver
    {
        public required string UserID { get; set; }
        public DateTime? LastOnline { get; set; }
        public double? GPSLat { get; set; }
        public double? GPSLong { get; set; }
        public DateTime? LocationLastUpdated { get; set; }
    }
}
