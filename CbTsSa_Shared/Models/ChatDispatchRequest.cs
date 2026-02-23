namespace CbTsSa_Shared.Models
{
    public class ChatDispatchRequest
    {
        public required ChatUpdate ChatUpdate { get; init; }
        public Guid CorrelationId { get; init; }
        public Dictionary<string, string>? Tags { get; init; } // e.g., "urgent", "order:update"
        public string? BusinessID { get; init; } // optional business routing
    }
}
