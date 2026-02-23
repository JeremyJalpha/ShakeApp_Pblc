using CbTsSa_Shared.DBModels;
using CbTsSa_Shared.CbTsSaConstants;

namespace CommandBot.Models
{
    public class BusinessContext
    {
        public long CellNumber { get; }
        public string BaseUrl { get; init; } = string.Empty;
        public Business? Business { get; init; }
        public string CommandMenu { get; init; } = "#MainMenu#";
        public string MenuFooter { get; init; } = string.Empty;
        public string Catalog { get; init; } = string.Empty;
        public string PrclstPreamble { get; init; } = string.Empty;
        public string NewUserGreeting_Cold { get; init; } = string.Empty;
        public string NewUserGreeting_Warm { get; init; } = string.Empty;
        public IReadOnlyList<CatalogItem> CatalogItems { get; init; } = Array.Empty<CatalogItem>();
        public ChatChannelType Channel { get; init; } = ChatChannelType.WhatsApp;

        public BusinessContext(long cellNumber, string baseurl, Func<string> commandMenuProvider, ChatChannelType chatChannelType)
        {
            CellNumber = cellNumber;
            BaseUrl = baseurl;
            CommandMenu = commandMenuProvider();
            Channel = chatChannelType;
        }
        public string PrclstAsAString() => string.Join('\n', CatalogItems);

        public string GetFullMenu()
        {
            var menu = CommandMenu;
            if (!string.IsNullOrWhiteSpace(MenuFooter))
            {
                menu += $"\n\n{MenuFooter}";
            }
            return menu;
        }
    }
}
