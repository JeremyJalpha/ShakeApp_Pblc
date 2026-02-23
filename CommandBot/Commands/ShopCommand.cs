using CommandBot.Attributes;
using CommandBot.Models;

namespace CommandBot.Commands
{
    [Command("shop", "Show shop details", showInMenu:true, groupNumber: 1, order: 2)]
    public class ShopCommand : BaseCommand
    {
        private readonly ILogger<ShopCommand> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public ShopCommand(ILogger<ShopCommand> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;

            CommandKey = "shop";
            Description = "Show shop details";
        }

        public override Task<string> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (context?.BusiContext == null)
            {
                _logger.LogWarning("Business context is missing when executing ShopCommand.");
                throw new ArgumentNullException("Business context is missing.");
            }

            // keep reference to factory for consistency with pattern
            _logger.LogDebug("HttpClientFactory available: {HasFactory}", _httpClientFactory != null);

            var priceList = context.BusiContext.PrclstAsAString();
            var catalog = context.BusiContext.CatalogItems;
            var result = !string.IsNullOrWhiteSpace(priceList)
                ? $"{context.BusiContext.PrclstPreamble}\n\n{catalog}"
                : "Shop information unavailable.";

            return Task.FromResult(result);
        }
    }
}