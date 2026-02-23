using CbTsSa_Shared.DBModels;
using CbTsSa_Shared.CbTsSaConstants;

namespace CommandBot.Models
{
    public class ConversationContext
    {
        public string MsgOriginNumber { get; private set; }
        public string MessageBody { get; private set; }
        public bool UserWasCreated { get; set; } = false;
        public ApplicationUser User { get; set; }
        public string? CurrentOrder { get; set; }

        public ChatChannelType Channel { get; init; }

        /// <summary>
        /// Platform-specific media handle for images/videos in the message
        /// </summary>
        public string MediaHandle { get; set; }

        public ConversationContext(ApplicationUser user, bool userWasCreated, string msgOriginNumber, string messageBody, ChatChannelType channel)
        {
            User = user ?? throw new ArgumentException("User cannot be null or empty.", nameof(user));

            UserWasCreated = userWasCreated;

            MsgOriginNumber = msgOriginNumber ?? throw new ArgumentException("Message origin number cannot be null or empty.", nameof(msgOriginNumber));

            MessageBody = messageBody ?? throw new ArgumentException("Message body cannot be null or empty.", nameof(messageBody));

            Channel = channel;
        }
    }
}