using CommandBot.Attributes;
using CommandBot.Models;

namespace CommandBot.Commands
{
    [Command("menu", "Show this menu", showInMenu: true, groupNumber: 1, order: 1)]
    public class MenuCommand : BaseCommand
    {
        private readonly ILogger<MenuCommand> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public MenuCommand(ILogger<MenuCommand> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;

            CommandKey = "menu";
            Description = "Show this menu";
        }

        public override Task<string> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (context?.BusiContext == null)
            {
                _logger.LogWarning("Business context is missing when executing MenuCommand.");
                throw new ArgumentNullException(nameof(context.BusiContext), "Business context is missing.");
            }

            // Keep reference to factory for pattern consistency
            _logger.LogDebug("HttpClientFactory available: {HasFactory}", _httpClientFactory != null);

            // Get the full menu including dynamic footer
            return Task.FromResult(context.BusiContext.GetFullMenu());
        }
    }
}