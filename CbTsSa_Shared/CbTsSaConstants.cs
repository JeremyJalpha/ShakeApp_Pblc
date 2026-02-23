using System.ComponentModel;

namespace CbTsSa_Shared.CbTsSaConstants
{
    public enum JwtTokenType
    {
        Unknown,
        ShortLived,
        LongLived
    }

    public enum ChatChannelType
    {
        None,
        WhatsApp,
        Telegram
    }

    public enum ChatMessageType
    {
        Text,
        Image,
        Document
    }

    public enum BroadcastTemplateStatus
    {
        [Description("Not Submitted")]
        NotSubmitted,
        [Description("Submitted")]
        Submitted,
        [Description("Approved")]
        Approved,
        [Description("Rejected")]
        Rejected
    }

    public static class BroadcastTemplateStatusExtensions
    {
        public static string GetDescription(this BroadcastTemplateStatus status)
        {
            var field = status.GetType().GetField(status.ToString());
            var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
            return attribute?.Description ?? status.ToString();
        }
    }

    public static class TokenStorageKeys
    {
        public const string Unknown = "unknown_jwt";
        public const string ShortLived = "short_lived_jwt";
        public const string LongLived = "long_lived_jwt";

        public static string ForType(JwtTokenType type) => type switch
        {
            JwtTokenType.ShortLived => ShortLived,
            JwtTokenType.LongLived => LongLived,
            _ => Unknown
        };
    }

    public static class BroadcastLimits
    {
        // Per-business limits
        public const int MaxCampaignImagesPerBusiness = 10; // Total across all users
        public const int MaxTemplatesPerBusiness = 10;
        public const int MaxActiveCampaignsPerBusiness = 10;

        // Per-user limits (simplified - 1 image per user)
        public const int MaxCampaignImagesPerUser = 1; // Each user can only have 1 active image
        public const int MaxTemplatesPerUserPerDay = 5;
        public const int MaxBroadcastsPerUserPerDay = 3;

        // Campaign size limits
        public const int MaxRecipientsPerCampaign = 10000;
        public const int MinRecipientsBatchSize = 1;

        // Time-based limits
        public static readonly TimeSpan MinTimeBetweenCampaigns = TimeSpan.FromHours(1);
        public static readonly TimeSpan ImageRetentionPeriod = TimeSpan.FromDays(90);
        public static readonly TimeSpan CompletedCampaignRetentionPeriod = TimeSpan.FromDays(30);
    }

    public enum UserClaim
    {
        None = 0,
        CanBroadcast = 1
    }

    public static class CbTsSaConstants
    {
        public const string LocationRequestGroup = "LocationRequest";
        public const string ToGPSHubSendLastKnownLocOfClient = "ToGPSHubSendLastKnownLocOfClient";
        public const string FromAuthHubGetLongLivedJWT = "FromAuthHubGetLongLivedJWT";
        public const string JoinLocationRequestGroup = "JoinLocationRequestGroup";
        public const string JoinLocationRequestGroupFailed = "JoinLocationRequestGroupFailed";
        public const string SendLastKnownLocOfClientFailed = "SendLastKnownLocOfClientFailed";
        public const string ExitLocationRequestGroup = "ExitLocationRequestGroup";
        public const string AuthHubUrl = "https://localhost:443/AuthenticationHub";
        public const string GPSHubUrl = "https://localhost:443/GPSLocationHub";
        public const string ExpectedIssuer = "https://localhost:443";
        public static readonly List<string> ExpectedAudiences = new List<string> { "https://localhost:443" };
        public const string MessageQueueHostName = "rabbitmq";
        public const string CommandQueueName = "command_queue";
        public const string TelegramOutboundQueue = "telegram_outbound_queue";
        public const string WhatsAppOutboundQueue = "whatsapp_outbound_queue";
        public const string ImageProcessingQueue = "image_processing_queue";
        public const string TokenPlaceHolder = "TOKEN_HERE";
        public const string DefaultGreeting_Cold = "Hello there, I'm ShakeApp, very nice to meet you. For the Main Menu please send: #menu";
        public const string DefaultGreeting_Warm = "Have we met before?\nYou seem to know your way around, but just in case for the Main Menu please send: #menu";
        public const string DefaultPriceListPreamble = "Welcome to the ShakeApp shop!";
        public const string NoCommandsFoundInMessage = "No commands found in message.";
        public const string MainMenuHint = "For the Main Menu please send: #menu";
        public const string SignUpFormBaseURL = "https://shakeapptest1-6b90e88bf538.herokuapp.com/signup";
    }
}
