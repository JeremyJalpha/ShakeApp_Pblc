using CommandBot.Attributes;
using CommandBot.Models;

namespace CommandBot.Commands
{
    /// <summary>
    /// Displays the user's current order in the full audit format.
    /// Shows items as: quantity:menuCode (modifications)
    /// Example: 1:M_024 (-E_004+E_001, +E_007)
    /// </summary>
    [Command("currentorder", "Show your current order", showInMenu: true, groupNumber: 1, order: 5)]
    [Command("current order", "Show your current order", groupNumber: 3, order: 1)]
    public class CurrentOrderCommand : BaseCommand
    {
        private readonly ILogger<CurrentOrderCommand> _logger;

        public CurrentOrderCommand(ILogger<CurrentOrderCommand> logger)
        {
            _logger = logger;
            CommandKey = "current order";
            Description = "Show your current order";
        }

        public override async Task<string> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (context?.ConvoContext?.User == null)
                return "❌ User not found.";

            try
            {
                var user = context.ConvoContext.User;
                var currentOrder = user.CurrentOrder;

                if (currentOrder == null || currentOrder.Count == 0)
                {
                    return "📋 Your order is empty.\n\nUse #update order to add items!";
                }

                var orderLines = new List<string>
                {
                    "📋 *Your Current Order:*",
                    ""
                };

                foreach (var item in currentOrder)
                {
                    orderLines.Add(item.GetFullOrderFormat());
                }

                orderLines.Add("");
                orderLines.Add($"Total items: {currentOrder.Count}");
                orderLines.Add($"Total quantity: {currentOrder.Sum(o => o.ItemAmount)}");

                _logger.LogInformation(
                    "User {UserId} viewed their order with {ItemCount} items",
                    user.Id,
                    currentOrder.Count);

                return string.Join("\n", orderLines);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve current order");
                return $"❌ Failed to retrieve order: {ex.Message}";
            }
        }
    }
}