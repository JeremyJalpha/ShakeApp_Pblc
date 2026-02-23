using System.Text.RegularExpressions;
using CbTsSa_Shared.Models;
using CommandBot.Attributes;
using CommandBot.Interfaces;
using CommandBot.Models;
using Microsoft.EntityFrameworkCore;

namespace CommandBot.Commands
{
    /// <summary>
    /// Updates the user's current order by adding, updating, or removing items.
    /// 
    /// Supports the new format only:
    /// #update order quantity:MenuCode [(modifications)]
    /// Examples:
    /// - Add/update: 1:M_024
    /// - With modifications: 1:M_024 (-E_004+E_001, +E_007)
    /// - Remove: 0:M_024
    /// </summary>
    [Command(@"update\s+order\s*:\s*(.+)",
             "#update order: <Quantity:MenuCode (modifications)>",
             showInMenu: true,
             groupNumber: 3,
             order: 2)]
    public class UpdateOrderCommand : BaseCommand, IPatternCommand
    {
        private string _orderData = string.Empty;
        private readonly ILogger<UpdateOrderCommand> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public UpdateOrderCommand(ILogger<UpdateOrderCommand> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;

            CommandKey = "update:order";
            Description = "Update your order";
        }

        public void Initialize(Match match)
        {
            _orderData = match.Groups[1].Value.Trim();
            CommandKey = "update:order";
            Description = "Update your order";
        }

        public override async Task<string> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogDebug("HttpClientFactory available for UpdateOrderCommand: {HasFactory}", _httpClientFactory != null);

            if (context?.ConvoContext?.User == null)
                return "❌ User not found.";

            if (context?.BusiContext?.Business == null)
                return "❌ Business context missing.";

            try
            {
                var user = context.ConvoContext.User;
                var businessId = context.BusiContext.Business.BusinessID;

                // Parse the order updates from the command (synchronous)
                var orderUpdates = ParseUpdateOrderCommand(_orderData);
                if (orderUpdates.Count == 0)
                    return "❌ Invalid order update format. Use: #update order quantity:MenuCode [(modifications)]";

                // Validate all menu codes exist in the business's catalog
                var menuCodes = orderUpdates.Select(o => o.MenuCode).Distinct().ToList();
                var validMenuCodes = await context.AppDbContext.CatalogItems
                    .Where(ci => ci.Catalog.BusinessID == businessId && menuCodes.Contains(ci.MenuCode))
                    .Select(ci => ci.MenuCode)
                    .ToListAsync(cancellationToken);

                var invalidCodes = menuCodes.Except(validMenuCodes).ToList();
                if (invalidCodes.Any())
                {
                    return $"❌ Invalid menu code(s): {string.Join(", ", invalidCodes)}";
                }

                // Get the current order list (in-memory on user object)
                var currentOrder = user.CurrentOrder ?? new List<OrderItems>();
                var results = new List<string>();

                foreach (var update in orderUpdates)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // Validate the new amount
                    if (update.ItemAmount < 0)
                    {
                        results.Add($"❌ Item {update.MenuCode}: quantity cannot be negative.");
                        continue;
                    }

                    // Find existing item in the order (match by MenuCode and Modifications)
                    var existingItem = FindExistingOrderItem(currentOrder, update);

                    if (update.ItemAmount == 0)
                    {
                        // Remove item from order
                        if (existingItem != null)
                        {
                            currentOrder.Remove(existingItem);
                            results.Add($"✓ Item {update.MenuCode} removed.");
                        }
                        else
                        {
                            results.Add($"⚠️ Item {update.MenuCode} not found in order.");
                        }
                    }
                    else
                    {
                        // Add or update item
                        if (existingItem != null)
                        {
                            existingItem.ItemAmount = update.ItemAmount;
                            existingItem.Modifications = update.Modifications;
                            results.Add($"✓ Item {update.MenuCode} updated to {update.ItemAmount}.");
                        }
                        else
                        {
                            currentOrder.Add(new OrderItems
                            {
                                MenuCode = update.MenuCode,
                                ItemAmount = update.ItemAmount,
                                Modifications = update.Modifications
                            });
                            results.Add($"✓ Item {update.MenuCode} added with amount {update.ItemAmount}.");
                        }
                    }
                }

                // Persist changes
                user.CurrentOrder = currentOrder;
                await context.AppDbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "User {UserId} updated order with {UpdateCount} changes",
                    user.Id,
                    results.Count);

                return string.Join("\n", results);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("UpdateOrderCommand cancelled for user");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update order");
                return $"❌ Failed to update order: {ex.Message}";
            }
        }

        private OrderItems? FindExistingOrderItem(List<OrderItems> currentOrder, OrderItems update)
        {
            // Match by MenuCode and exact Modifications (including null)
            return currentOrder.FirstOrDefault(o =>
                string.Equals(o.MenuCode, update.MenuCode, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(o.Modifications ?? string.Empty, update.Modifications ?? string.Empty, StringComparison.Ordinal));
        }

        private List<OrderItems> ParseUpdateOrderCommand(string orderData)
        {
            var orderUpdates = new List<OrderItems>();

            try
            {
                // Use a regex to find all top-level item matches (quantity:MenuCode (optional modifications))
                // This avoids splitting on commas which may appear inside the modifications parentheses.
                var pattern = @"(?:(\d+)\s*:\s*([A-Z]_[0-9]{1,3})(?:\s*\((.*?)\))?)";
                var matches = Regex.Matches(orderData, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);

                foreach (Match m in matches)
                {
                    if (!m.Success)
                        continue;

                    var quantity = int.Parse(m.Groups[1].Value);
                    var menuCode = m.Groups[2].Value.ToUpperInvariant();
                    // Treat empty parentheses () same as no parentheses - both result in null modifications
                    var modifications = m.Groups[3].Success && !string.IsNullOrWhiteSpace(m.Groups[3].Value)
                        ? m.Groups[3].Value.Trim()
                        : null;

                    orderUpdates.Add(new OrderItems
                    {
                        MenuCode = menuCode,
                        ItemAmount = quantity,
                        Modifications = modifications
                    });
                }

                if (orderUpdates.Count == 0)
                {
                    throw new ArgumentException("No valid order items found in input.");
                }

                return orderUpdates;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse order data: {OrderData}", orderData);
                throw new ArgumentException($"Invalid order format. {ex.Message}");
            }
        }
    }
}