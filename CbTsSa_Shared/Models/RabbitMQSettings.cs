namespace CbTsSa_Shared.Models
{
    public class RabbitMqSettings
    {
        public string? ConnectionString { get; set; }
        public string Host { get; set; } = "-1";
        public string Username { get; set; } = "-1";
        public string Password { get; set; } = "-1";
        public int Port { get; set; } = -1;
    }
}
