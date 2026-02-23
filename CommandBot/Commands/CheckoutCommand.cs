using System.Security.Cryptography;
using System.Text;
using CbTsSa_Shared.DBModels;
using CbTsSa_Shared.Models;
using CommandBot.Attributes;
using CommandBot.Models;
using Microsoft.EntityFrameworkCore;

namespace CommandBot.Commands
{
    /// <summary>
    /// Tallies the user's current order, creates a Sale record, and initiates
    /// payment via PayFast. Returns a payment URL for the user to complete checkout.
    ///
    /// Pricing logic mirrors the original Go CalculatePrice / BeginCheckout flow:
    /// - SingleItem: quantity × BasePrice
    /// - WeightItem: weight  × BasePrice (per-gram pricing)
    /// - Active Specials apply percentage discounts
    ///
    /// MenuCode resolution: numeric suffix maps to ItemID (e.g. "M_005" → ItemID 5).
    /// </summary>
    [Command("checkout", "Checkout and pay for your current order", showInMenu: true, groupNumber: 1, order: 6)]
    public class CheckoutCommand : BaseCommand
    {
        private readonly ILogger<CheckoutCommand> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public CheckoutCommand(
            ILogger<CheckoutCommand> logger,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            CommandKey = "checkout";
            Description = "Checkout and pay for your current order";
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
                    return "📋 Your order is empty.\n\nUse #update order to add items first!";

                var business = context.BusiContext.Business;
                if (business == null)
                    return "❌ Business not configured.";

                // ── Tally the order ──────────────────────────────────────
                var (cartTotal, cartLines, errors) = await TallyOrderAsync(
                    context, currentOrder, business.BusinessID, cancellationToken);

                if (errors.Count > 0 && cartLines.Count == 0)
                    return "❌ Could not process any order items:\n" + string.Join("\n", errors);

                // ── Create Sale record ───────────────────────────────────
                var sale = new Sale
                {
                    UserID = user.Id,
                    DtTmRequestedCheckout = DateTime.UtcNow,
                    ItemCount = currentOrder.Sum(o => o.ItemAmount),
                    TotalAmount = cartTotal,
                    IsAmtManual = false
                };
                context.AppDbContext.Sales.Add(sale);
                await context.AppDbContext.SaveChangesAsync(cancellationToken);

                // ── Create initial Payment record ────────────────────────
                var payment = new Payment
                {
                    SaleID = sale.SaleID,
                    SenderEmail = user.Email,
                    Amount = cartTotal,
                    CurrencyCode = "ZAR",
                    InitiationDateTime = DateTime.UtcNow
                };
                context.AppDbContext.Payments.Add(payment);
                await context.AppDbContext.SaveChangesAsync(cancellationToken);

                // ── Process payment via PayFast ──────────────────────────
                var itemNamePrefix = _configuration["PayFast:ItemNamePrefix"] ?? "Order_";
                var itemName = $"{itemNamePrefix}{sale.SaleID}";

                var paymentUrl = await ProcessPaymentAsync(
                    sale, user, cartTotal, itemName, cancellationToken);

                // ── Build response message ───────────────────────────────
                var response = new List<string>
                {
                    "🛒 *Checkout Summary:*",
                    ""
                };
                response.AddRange(cartLines);

                if (errors.Count > 0)
                {
                    response.Add("");
                    response.AddRange(errors);
                }

                response.Add("");
                response.Add($"*Total: R{cartTotal:F2}*");
                response.Add("");

                if (!string.IsNullOrWhiteSpace(paymentUrl)
                    && paymentUrl != "Checkout initiation failed")
                {
                    response.Add($"💳 Complete your payment here:\n{paymentUrl}");
                }
                else
                {
                    response.Add("❌ Payment initiation failed. Please try again or contact support.");
                }

                _logger.LogInformation(
                    "User {UserId} initiated checkout: {ItemCount} items, total R{Total:F2}, SaleID {SaleId}",
                    user.Id, currentOrder.Count, cartTotal, sale.SaleID);

                return string.Join("\n", response);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("CheckoutCommand cancelled for user");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process checkout");
                return $"❌ Failed to process checkout: {ex.Message}";
            }
        }

        // ── Order tallying ───────────────────────────────────────────────

        /// <summary>
        /// Resolves each order item's MenuCode through the entity chain
        /// (CatalogItem → Item → Purchasable → Offer → OfferType) and calculates pricing.
        /// Applies active Special discounts when applicable.
        /// Also processes modifications (extras) from the Modifications string.
        /// </summary>
        private async Task<(decimal Total, List<string> Lines, List<string> Errors)> TallyOrderAsync(
            CommandContext context,
            List<OrderItems> orderItems,
            long businessId,
            CancellationToken cancellationToken)
        {
            decimal cartTotal = 0;
            var cartLines = new List<string>();
            var errors = new List<string>();

            foreach (var orderItem in orderItems)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Look up the main item by MenuCode
                var catalogItem = await context.AppDbContext.CatalogItems
                    .Include(ci => ci.Item)
                        .ThenInclude(i => i.Purchasable)
                            .ThenInclude(p => p.Offer)
                                .ThenInclude(o => o.OfferType)
                    .Include(ci => ci.Item)
                        .ThenInclude(i => i.Purchasable)
                            .ThenInclude(p => p.Saleable)
                                .ThenInclude(s => s.Product!)
                                    .ThenInclude(pr => pr.Good)
                    .Include(ci => ci.Item)
                        .ThenInclude(i => i.Special)
                    .Where(ci => ci.Catalog.BusinessID == businessId
                              && ci.MenuCode == orderItem.MenuCode)
                    .FirstOrDefaultAsync(cancellationToken);

                if (catalogItem == null)
                {
                    errors.Add($"⚠️ Item {orderItem.MenuCode} not found in catalog.");
                    continue;
                }

                var item = catalogItem.Item;
                var offer = item.Purchasable.Offer;
                var basePrice = offer.BasePrice ?? 0m;
                var offerTypeName = offer.OfferType.TypeName;
                var goodName = item.Purchasable.Saleable.Product?.Good?.Name
                               ?? orderItem.MenuCode;

                decimal itemTotal;
                switch (offerTypeName)
                {
                    case "WeightItem":
                        // WeightItem: ItemAmount represents grams, BasePrice is per-gram
                        itemTotal = orderItem.ItemAmount * basePrice;
                        cartLines.Add(
                            $"  {orderItem.ItemAmount}g {goodName} @ R{basePrice:F2}/g = R{itemTotal:F2}");
                        break;

                    case "SingleItem":
                    default:
                        itemTotal = orderItem.ItemAmount * basePrice;
                        cartLines.Add(
                            $"  {orderItem.ItemAmount}x {goodName} @ R{basePrice:F2} = R{itemTotal:F2}");
                        break;
                }

                // Process modifications (extras)
                if (!string.IsNullOrWhiteSpace(orderItem.Modifications))
                {
                    var (modTotal, modLines, modErrors) = await ProcessModificationsAsync(
                        context, orderItem.Modifications, orderItem.ItemAmount, businessId, cancellationToken);

                    itemTotal += modTotal;
                    cartLines.AddRange(modLines);
                    errors.AddRange(modErrors);
                }

                // Apply active Special discount (percentage-based)
                var special = item.Special;
                if (special is { IsActive: true, Discount: not null }
                    && DateTime.UtcNow >= special.SpecialDtTmStart
                    && DateTime.UtcNow <= special.SpecialDtTmEnd)
                {
                    var discount = itemTotal * (special.Discount.Value / 100m);
                    itemTotal -= discount;
                    cartLines.Add(
                        $"    🏷️ {special.SpecialName}: -{special.Discount.Value}% (-R{discount:F2})");
                }

                cartTotal += itemTotal;
            }

            return (cartTotal, cartLines, errors);
        }

        /// <summary>
        /// Parses and prices modifications from a string like "(-E_004+E_001, +E_007)".
        /// Format: +MenuCode adds, -MenuCode removes (but we charge for adds only).
        /// Returns total price for modifications, formatted lines, and any errors.
        /// </summary>
        private async Task<(decimal Total, List<string> Lines, List<string> Errors)> ProcessModificationsAsync(
            CommandContext context,
            string modificationsString,
            int quantity,
            long businessId,
            CancellationToken cancellationToken)
        {
            decimal modTotal = 0;
            var modLines = new List<string>();
            var modErrors = new List<string>();

            // Parse modification codes: (+E_001, -E_004, +E_007)
            // Remove parentheses, split by comma, trim each entry
            var modString = modificationsString.Trim('(', ')', ' ');
            var modEntries = modString.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(m => m.Trim())
                .ToList();

            foreach (var modEntry in modEntries)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Parse +/- prefix and menu code
                var isAddition = modEntry.StartsWith('+');
                var isRemoval = modEntry.StartsWith('-');

                if (!isAddition && !isRemoval)
                {
                    modErrors.Add($"    ⚠️ Invalid modification format: {modEntry}");
                    continue;
                }

                var menuCode = modEntry[1..].Trim(); // Remove +/- prefix

                // Only charge for additions, removals are free
                if (!isAddition)
                {
                    modLines.Add($"    {modEntry} (no charge)");
                    continue;
                }

                // Look up the modification item
                var modCatalogItem = await context.AppDbContext.CatalogItems
                    .Include(ci => ci.Item)
                        .ThenInclude(i => i.Purchasable)
                            .ThenInclude(p => p.Offer)
                                .ThenInclude(o => o.OfferType)
                    .Include(ci => ci.Item)
                        .ThenInclude(i => i.Purchasable)
                            .ThenInclude(p => p.Saleable)
                                .ThenInclude(s => s.Product!)
                                    .ThenInclude(pr => pr.Good)
                    .Where(ci => ci.Catalog.BusinessID == businessId
                              && ci.MenuCode == menuCode)
                    .FirstOrDefaultAsync(cancellationToken);

                if (modCatalogItem == null)
                {
                    modErrors.Add($"    ⚠️ Modification {menuCode} not found in catalog.");
                    continue;
                }

                var modOffer = modCatalogItem.Item.Purchasable.Offer;
                var modPrice = modOffer.BasePrice ?? 0m;
                var modName = modCatalogItem.Item.Purchasable.Saleable.Product?.Good?.Name
                              ?? menuCode;

                // Multiply by quantity of the parent item
                var modLineTotal = quantity * modPrice;
                modTotal += modLineTotal;

                modLines.Add($"    +{modName} @ R{modPrice:F2} × {quantity} = R{modLineTotal:F2}");
            }

            return (modTotal, modLines, modErrors);
        }

        // ── PayFast integration ──────────────────────────────────────────

        /// <summary>
        /// Builds a signed PayFast payment request and POSTs it.
        /// Mirrors the Go ProcessPayment function: URL-encode params for signature only,
        /// then POST unencoded values (FormUrlEncodedContent handles encoding).
        /// Captures 3xx redirect Location header.
        /// </summary>
        private async Task<string> ProcessPaymentAsync(
            Sale sale,
            ApplicationUser user,
            decimal cartTotal,
            string itemName,
            CancellationToken cancellationToken)
        {
            var merchantId  = _configuration["PayFast:MerchantId"] ?? string.Empty;
            var merchantKey = _configuration["PayFast:MerchantKey"] ?? string.Empty;
            var passphrase  = _configuration["PayFast:Passphrase"] ?? string.Empty;
            var hostUrl     = _configuration["PayFast:HostUrl"]
                              ?? "https://www.payfast.co.za/eng/process";
            var returnUrl   = _configuration["PayFast:ReturnUrl"] ?? string.Empty;
            var cancelUrl   = _configuration["PayFast:CancelUrl"] ?? string.Empty;
            var notifyUrl   = _configuration["PayFast:NotifyUrl"] ?? string.Empty;

            var cellNumber = user.UserIndicatedCell ?? string.Empty;
            var email = user.Email ?? string.Empty;

            // Build parameters in PayFast's required order, excluding empty values
            // Keep values UNENCODED - FormUrlEncodedContent will encode them
            var parameters = new List<KeyValuePair<string, string>>
            {
                new("merchant_id",   merchantId),
                new("merchant_key",  merchantKey),
                new("return_url",    returnUrl),
                new("cancel_url",    cancelUrl),
                new("notify_url",    notifyUrl),
                new("name_first",    user.UserName ?? string.Empty),
                new("name_last",     cellNumber),
                new("email_address", email),
                new("cell_number",   cellNumber),
                new("m_payment_id",  sale.SaleID.ToString()),
                new("amount",        cartTotal.ToString("F2")),
                new("item_name",     itemName)
            }
            // Exclude empty parameters as per PayFast requirements
            .Where(p => !string.IsNullOrWhiteSpace(p.Value))
            .ToList();

            // Generate MD5 signature (matching Go GenerateSignature logic)
            // Signature calculation uses URL-encoded values
            var signature = GenerateSignature(parameters, passphrase);

            _logger.LogInformation(
                "Generated PayFast signature: {Signature} for SaleID {SaleId}",
                signature, sale.SaleID);

            // Add signature to parameters (unencoded, FormUrlEncodedContent will encode it)
            parameters.Add(new("signature", signature));

            // Log parameters being sent (excluding sensitive data in production)
            _logger.LogDebug(
                "PayFast parameters: {Parameters}",
                string.Join(", ", parameters.Select(p => $"{p.Key}={p.Value}")));

            // POST to PayFast using named client (auto-redirect disabled)
            // FormUrlEncodedContent will properly encode the unencoded parameter values
            var client = _httpClientFactory.CreateClient("PayFast");
            var content = new FormUrlEncodedContent(parameters);

            var response = await client.PostAsync(hostUrl, content, cancellationToken);

            // PayFast returns 302 redirect to the payment page
            if ((int)response.StatusCode >= 300 && (int)response.StatusCode < 400)
            {
                var redirectUrl = response.Headers.Location?.ToString();
                if (!string.IsNullOrWhiteSpace(redirectUrl))
                    return redirectUrl;
            }

            // Log error response body for debugging
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning(
                "PayFast returned unexpected status {StatusCode} for SaleID {SaleId}. Response: {ResponseBody}",
                response.StatusCode, sale.SaleID, responseBody);

            return "Checkout initiation failed";
        }

        /// <summary>
        /// Generates an MD5 signature from URL-encoded key=value pairs + passphrase.
        /// Uses RFC 3986 encoding (spaces as %20) to match Go's url.QueryEscape behavior.
        /// </summary>
        private static string GenerateSignature(
            List<KeyValuePair<string, string>> parameters,
            string passphrase)
        {
            // URL-encode parameters using RFC 3986 (spaces as %20)
            // Keys are not encoded as they're always alphanumeric with underscores
            var paramString = string.Join("&",
                parameters.Select(p =>
                    $"{p.Key}={Uri.EscapeDataString(p.Value)}"));

            // Append passphrase if provided
            if (!string.IsNullOrWhiteSpace(passphrase))
                paramString += $"&passphrase={Uri.EscapeDataString(passphrase)}";

            var hashBytes = MD5.HashData(Encoding.UTF8.GetBytes(paramString));
            return Convert.ToHexStringLower(hashBytes);
        }
    }
}