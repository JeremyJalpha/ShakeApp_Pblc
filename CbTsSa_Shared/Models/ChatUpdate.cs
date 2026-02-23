using CbTsSa_Shared.DBModels;
using CbTsSa_Shared.CbTsSaConstants;

public class ChatUpdate
{
    // Required properties
    public required ApplicationUser From { get; set; }
    public required string Body { get; set; }
    public required ChatChannelType Channel { get; init; }

    // Message type and media handling
    public ChatMessageType MessageType { get; set; } = ChatMessageType.Text;

    /// <summary>
    /// Platform-specific media identifier (WhatsApp media ID or Telegram file_id).
    /// This is the primary way to reference media in messages.
    /// </summary>
    public string? MediaHandle { get; set; }

    /// <summary>
    /// Optional caption for media messages (images, videos, documents).
    /// </summary>
    public string? Caption { get; set; }

    /// <summary>
    /// Deprecated: Use MediaHandle for platform-native media references instead.
    /// This property exists for backward compatibility only.
    /// </summary>
    [Obsolete("Use MediaHandle for platform-native media references. This will be removed in a future version.")]
    public string? ImageUrl { get; set; }
}