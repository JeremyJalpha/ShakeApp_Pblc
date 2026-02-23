using System.Text.Json.Serialization;

namespace CbTsSa_Shared.Models.DTOs
{
    public class WebhookRequest
    {
        [JsonPropertyName("object")]
        public string? Object { get; set; }

        [JsonPropertyName("entry")]
        public List<WebhookEntry>? Entry { get; set; }
    }

    public class WebhookEntry
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("changes")]
        public List<WebhookChange>? Changes { get; set; }
    }

    public class WebhookChange
    {
        [JsonPropertyName("value")]
        public WebhookValue? Value { get; set; }

        [JsonPropertyName("field")]
        public string? Field { get; set; }
    }

    public class WebhookValue
    {
        [JsonPropertyName("messaging_product")]
        public string? MessagingProduct { get; set; }

        [JsonPropertyName("metadata")]
        public Metadata? Metadata { get; set; }

        [JsonPropertyName("contacts")]
        public List<Contact>? Contacts { get; set; }

        [JsonPropertyName("messages")]
        public List<Message>? Messages { get; set; }

        [JsonPropertyName("statuses")]
        public List<Status>? Statuses { get; set; }
    }

    public class Metadata
    {
        [JsonPropertyName("display_phone_number")]
        public string? DisplayPhoneNumber { get; set; }

        [JsonPropertyName("phone_number_id")]
        public string? PhoneNumberId { get; set; }
    }

    public class Contact
    {
        [JsonPropertyName("profile")]
        public ContactProfile? Profile { get; set; }

        [JsonPropertyName("wa_id")]
        public string? WaId { get; set; }
    }

    public class ContactProfile
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    public class Message
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("from")]
        public string? From { get; set; }

        [JsonPropertyName("timestamp")]
        public string? Timestamp { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("text")]
        public TextContent? Text { get; set; }

        [JsonPropertyName("image")]
        public ImageContent? Image { get; set; }

        // Extend with other types like document, etc.
    }

    public class TextContent
    {
        [JsonPropertyName("body")]
        public string? Body { get; set; }
    }

    public class ImageContent
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("mime_type")]
        public string? MimeType { get; set; }

        [JsonPropertyName("sha256")]
        public string? Sha256 { get; set; }

        [JsonPropertyName("caption")]
        public string? Caption { get; set; }
    }

    public class WhatsAppSendImageRequest
    {
        [JsonPropertyName("messaging_product")]
        public string MessagingProduct { get; set; } = "whatsapp";

        [JsonPropertyName("to")]
        public string? To { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; } = "image";

        [JsonPropertyName("image")]
        public WhatsAppImagePayload? Image { get; set; }
    }

    public class WhatsAppImagePayload
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; } // Media ID from WhatsApp upload

        [JsonPropertyName("link")]
        public string? Link { get; set; } // URL to the image (alternative to ID)

        [JsonPropertyName("caption")]
        public string? Caption { get; set; }
    }

    public class Status
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("status")]
        public string? StatusText { get; set; }

        [JsonPropertyName("timestamp")]
        public string? Timestamp { get; set; }

        [JsonPropertyName("recipient_id")]
        public string? RecipientId { get; set; }

        [JsonPropertyName("conversation")]
        public Conversation? Conversation { get; set; }

        [JsonPropertyName("pricing")]
        public Pricing? Pricing { get; set; }
    }

    public class Conversation
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("expiration_timestamp")]
        public string? ExpirationTimestamp { get; set; }

        [JsonPropertyName("origin")]
        public Origin? Origin { get; set; }
    }

    public class Origin
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }
    }

    public class Pricing
    {
        [JsonPropertyName("billable")]
        public bool Billable { get; set; }

        [JsonPropertyName("pricing_model")]
        public string? PricingModel { get; set; }

        [JsonPropertyName("category")]
        public string? Category { get; set; }
    }

    public class WhatsAppApiResponse
    {
        public bool Success { get; set; }
        public string TemplateId { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class WhatsAppTemplateResponse
    {
        public string id { get; set; }
        public string status { get; set; }
    }

    public class WhatsAppErrorResponse
    {
        public WhatsAppError error { get; set; }
    }

    public class WhatsAppError
    {
        public string message { get; set; }
        public int code { get; set; }
    }

    public class TemplateStatusResult
    {
        public string Status { get; set; }
        public string RejectionReason { get; set; }
    }

    public class WhatsAppTemplateStatusResponse
    {
        public string status { get; set; }
        public string rejected_reason { get; set; }
    }
}
