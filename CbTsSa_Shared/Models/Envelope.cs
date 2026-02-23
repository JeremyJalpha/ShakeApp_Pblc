public class Envelope
{
    public ChatUpdate ChatUpdate { get; set; } = default!;
    public string? CorrelationId { get; set; }
    public Dictionary<string, string>? Tags { get; set; }
    public string? BusinessID { get; set; }
}