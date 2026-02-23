namespace CommandBot.Models
{
    /// <summary>
    /// Configuration for PayFast payment gateway.
    /// Bind from environment variables using double-underscore notation:
    ///   PayFast__MerchantId, PayFast__MerchantKey, PayFast__Passphrase, etc.
    /// </summary>
    public class PayFastSettings
    {
        public const string SectionName = "PayFast";

        public string MerchantId { get; set; } = string.Empty;
        public string MerchantKey { get; set; } = string.Empty;
        public string Passphrase { get; set; } = string.Empty;
        public string HostUrl { get; set; } = "https://www.payfast.co.za/eng/process";
        public string ReturnUrl { get; set; } = string.Empty;
        public string CancelUrl { get; set; } = string.Empty;
        public string NotifyUrl { get; set; } = string.Empty;
        public string ItemNamePrefix { get; set; } = "Order_";
    }
}