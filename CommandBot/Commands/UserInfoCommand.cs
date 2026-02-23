using CommandBot.Attributes;
using CommandBot.Models;

namespace CommandBot.Commands
{
    [Command("userinfo", "Show user information", showInMenu: true, groupNumber: 1, order: 3)]
    [Command("user info", "Show user information", groupNumber: 1, order: 3)]
    public class UserInfoCommand : BaseCommand
    {
        private readonly ILogger<UserInfoCommand> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public UserInfoCommand(ILogger<UserInfoCommand> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;

            CommandKey = "userinfo";
            Description = "Show user information";
        }

        public override Task<string> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
        {
            if (context?.ConvoContext == null)
            {
                _logger.LogWarning("Conversation context is missing when running UserInfoCommand.");
                throw new ArgumentNullException(nameof(context.ConvoContext), "Conversation context is missing.");
            }

            var info = context.ConvoContext.User?.GetUserInfoAsAString() ?? "No user info found.";
            return Task.FromResult(info);
        }
    }
}