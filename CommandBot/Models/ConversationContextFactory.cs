using CommandBot.Interfaces;

namespace CommandBot.Models
{
    // Simplified factory that only handles context creation
    public class ConversationContextFactory
    {
        private readonly IUserService _userService;
        private readonly ILogger<ConversationContextFactory> _logger;

        public ConversationContextFactory(IUserService userService, ILogger<ConversationContextFactory> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        public async Task<ConversationContext> CreateConversationContextAsync(ChatUpdate chatUpdate)
        {
            // Validate input
            ValidateChatUpdate(chatUpdate);

            // 🔍 ADD THIS LOGGING
            _logger.LogInformation(
                "🔄 Creating ConversationContext - MediaHandle: '{MediaHandle}', MessageType: {MessageType}",
                chatUpdate.MediaHandle ?? "NULL",
                chatUpdate.MessageType
            );

            // Get or create user through the service
            var (user, wasCreated) = await _userService.GetOrCreateUserAsync(chatUpdate.From.CellNumber);
            if (user == null)
            {
                _logger.LogWarning("User not found or created.");
                throw new InvalidOperationException("User not found or created.");
            }

            var convo = new ConversationContext(user, wasCreated, chatUpdate.From.CellNumber, chatUpdate.Body, chatUpdate.Channel);
            
            // Set MediaHandle if present
            if (!string.IsNullOrWhiteSpace(chatUpdate.MediaHandle))
            {
                convo.MediaHandle = chatUpdate.MediaHandle;
                
                // 🔍 ADD THIS LOGGING
                _logger.LogInformation("✅ MediaHandle set on ConversationContext: '{MediaHandle}'", convo.MediaHandle);
            }
            else
            {
                // 🔍 ADD THIS LOGGING
                _logger.LogWarning("⚠️ No MediaHandle to set on ConversationContext");
            }

            return convo;
        }

        private static void ValidateChatUpdate(ChatUpdate chatUpdate)
        {
            if (string.IsNullOrWhiteSpace(chatUpdate.From?.CellNumber))
            {
                throw new ArgumentException("Cell number cannot be null or empty.", nameof(chatUpdate.From.CellNumber));
            }

            if (string.IsNullOrWhiteSpace(chatUpdate.Body))
            {
                throw new ArgumentException("Message body cannot be null or empty.", nameof(chatUpdate.Body));
            }
        }
    }
}
