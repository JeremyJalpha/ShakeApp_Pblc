using CbTsSa_Shared.DBModels;
using CbTsSa_Shared.Interfaces;
using CbTsSa_Shared.CbTsSaConstants;
using CommandBot.Services;

namespace CommandBot.Models
{
    public class BusinessContextFactory
    {
        private readonly ILogger<BusinessContextFactory> _logger;
        private readonly CommandRegistry _commandRegistry;

        public BusinessContextFactory(
            ILogger<BusinessContextFactory> logger,
            CommandRegistry commandRegistry)
        {
            _logger = logger;
            _commandRegistry = commandRegistry;
        }

        /// <summary>
        /// Gets the dynamically generated command menu from the registry.
        /// This replaces the old hardcoded menu.
        /// </summary>
        private string GetCommandMenu() => _commandRegistry.GenerateMenu();

        public BusinessContext CreateBusinessContext(
            IAppDbContext dbContext, 
            string baseurl, 
            string cellNumber, 
            ChatChannelType chatChannelType)
        {
            if (!long.TryParse(cellNumber, out long parsedCellNumber))
            {
                throw new ArgumentException("Cellnumber is not a valid integer.", nameof(cellNumber));
            }

            // Start with a default context
            var businessContext = new BusinessContext(
                parsedCellNumber, 
                baseurl, 
                GetCommandMenu, 
                chatChannelType);

            // Try to fetch the business
            var business = dbContext.Businesses.FirstOrDefault(b => b.Cellnumber.ToString() == cellNumber);
            if (business == null)
            {
                _logger.LogWarning(
                    "No business found for Cellnumber {CellNumber}. Creating default business context.", 
                    cellNumber);
                return businessContext;
            }

            // Fetch the catalog for the business, if available
            var catalog = dbContext.Catalogs.FirstOrDefault(c => c.BusinessID == business.BusinessID);
            List<CatalogItem> catalogItems = new List<CatalogItem>();
            
            if (catalog == null)
            {
                _logger.LogWarning(
                    "No catalog found for business with Cellnumber {CellNumber}.", 
                    cellNumber);
            }
            else
            {
                catalogItems = dbContext.CatalogItems
                    .Where(ci => ci.CatalogID == catalog.CatalogID)
                    .ToList();
            }

            // Fetch the pricelist preamble from the business record
            string preamble = business.PricelistPreamble ?? string.Empty;
            if (string.IsNullOrWhiteSpace(preamble))
            {
                _logger.LogWarning(
                    "Pricelist Preamble is not set for business with Cellnumber {CellNumber}. Using default.", 
                    cellNumber);
                preamble = CbTsSaConstants.DefaultPriceListPreamble;
            }

            // Fetch the new user greeting from the business record
            string newUserGreeting = business.NewUserGreeting_Cold ?? string.Empty;
            if (string.IsNullOrWhiteSpace(newUserGreeting))
            {
                _logger.LogWarning(
                    "New User Greeting is not set for business with Cellnumber {CellNumber}. Using default.", 
                    cellNumber);
                newUserGreeting = CbTsSaConstants.DefaultGreeting_Cold;
            }

            // Build signup URL with business name as FirstSignedUpWith parameter
            string menuFooter = string.Empty;
            if (!string.IsNullOrWhiteSpace(business.BusinessName))
            {
                var encodedBusinessName = Uri.EscapeDataString(business.BusinessName);
                var signupUrl = $"{CbTsSa_Shared.CbTsSaConstants.CbTsSaConstants.SignUpFormBaseURL}?FirstSignedUpWith={encodedBusinessName}";
                menuFooter = $"📝 New here? Sign up: {signupUrl}";

                _logger.LogInformation(
                    "Generated signup URL for business {BusinessName}: {Url}",
                    business.BusinessName,
                    signupUrl);
            }

            // Create a fully populated BusinessContext with the gathered data.
            businessContext = new BusinessContext(
                parsedCellNumber, 
                baseurl, 
                GetCommandMenu, 
                chatChannelType)
            {
                Business = business,
                Catalog = string.Empty,
                PrclstPreamble = preamble,
                NewUserGreeting_Cold = newUserGreeting,
                CatalogItems = catalogItems,
                MenuFooter = menuFooter
            };

            return businessContext;
        }
    }
}