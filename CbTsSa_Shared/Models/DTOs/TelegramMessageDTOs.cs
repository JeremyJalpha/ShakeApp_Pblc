using System.Text.Json.Serialization;

namespace CbTsSa_Shared.Models.DTOs
{
    public class TelegramUpdate
    {
        [JsonPropertyName("update_id")]
        public long UpdateId { get; set; }

        [JsonPropertyName("message")]
        public TelegramMessage? Message { get; set; }

        [JsonPropertyName("edited_message")]
        public TelegramMessage? EditedMessage { get; set; }

        [JsonPropertyName("callback_query")]
        public TelegramCallbackQuery? CallbackQuery { get; set; }
    }

    public class TelegramMessage
    {
        [JsonPropertyName("message_id")]
        public int MessageId { get; set; }

        [JsonPropertyName("from")]
        public TelegramUser? From { get; set; }

        [JsonPropertyName("chat")]
        public TelegramChat? Chat { get; set; }

        [JsonPropertyName("date")]
        public int Date { get; set; }

        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("photo")]
        public List<TelegramPhotoSize>? Photo { get; set; }

        [JsonPropertyName("caption")]
        public string? Caption { get; set; }

        [JsonPropertyName("entities")]
        public List<TelegramMessageEntity>? Entities { get; set; }

        [JsonPropertyName("reply_to_message")]
        public TelegramMessage? ReplyToMessage { get; set; }
    }

    public class TelegramUser
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("is_bot")]
        public bool IsBot { get; set; }

        [JsonPropertyName("first_name")]
        public string? FirstName { get; set; }

        [JsonPropertyName("last_name")]
        public string? LastName { get; set; }

        [JsonPropertyName("username")]
        public string? Username { get; set; }

        [JsonPropertyName("language_code")]
        public string? LanguageCode { get; set; }
    }

    public class TelegramChat
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("username")]
        public string? Username { get; set; }

        [JsonPropertyName("first_name")]
        public string? FirstName { get; set; }

        [JsonPropertyName("last_name")]
        public string? LastName { get; set; }
    }

    public class TelegramMessageEntity
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("offset")]
        public int Offset { get; set; }

        [JsonPropertyName("length")]
        public int Length { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("user")]
        public TelegramUser? User { get; set; }
    }

    public class TelegramCallbackQuery
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("from")]
        public TelegramUser? From { get; set; }

        [JsonPropertyName("message")]
        public TelegramMessage? Message { get; set; }

        [JsonPropertyName("chat_instance")]
        public string? ChatInstance { get; set; }

        [JsonPropertyName("data")]
        public string? Data { get; set; }
    }

    public class TelegramPhotoSize
    {
        [JsonPropertyName("file_id")]
        public string? FileId { get; set; }

        [JsonPropertyName("file_unique_id")]
        public string? FileUniqueId { get; set; }

        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }

        [JsonPropertyName("file_size")]
        public int? FileSize { get; set; }
    }

    public class TelegramSendPhotoRequest
    {
        [JsonPropertyName("chat_id")]
        public long ChatId { get; set; }

        [JsonPropertyName("photo")]
        public string? Photo { get; set; } // Can be file_id, URL, or file upload

        [JsonPropertyName("caption")]
        public string? Caption { get; set; }

        [JsonPropertyName("parse_mode")]
        public string? ParseMode { get; set; } // "HTML", "Markdown", etc.

        [JsonPropertyName("reply_to_message_id")]
        public int? ReplyToMessageId { get; set; }
    }

    public class TelegramWebhookUpdate
    {
        [JsonPropertyName("update_id")]
        public long UpdateId { get; set; }

        [JsonPropertyName("message")]
        public TelegramMessage? Message { get; set; }
    }

    public class TelegramSendMessageRequest
    {
        public string chat_id { get; set; }
        public string text { get; set; }
        public string parse_mode { get; set; }
    }
}
