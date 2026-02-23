namespace CbTsSa_Shared.Models
{
    public class JwtIssueConfig
    {
        public string Key { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public List<string> Audiences { get; set; } = new();
        public int TokenLifetimeMinutes { get; set; } = 30;
    }
}
