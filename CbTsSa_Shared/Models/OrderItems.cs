namespace CbTsSa_Shared.Models
{
    public class OrderItems
    {
        public int ItemAmount { get; set; }
        public required string MenuCode { get; set; }
        public string? Modifications { get; set; }
        
        /// <summary>
        /// Returns the full order format for persistence/audit: "quantity:menuCode (modifications)"
        /// Example: "1:M_024 (-E_004+E_001, +E_007)"
        /// </summary>
        public string GetFullOrderFormat()
        {
            var format = $"{ItemAmount}:{MenuCode}";
            
            if (!string.IsNullOrWhiteSpace(Modifications))
            {
                format += $" ({Modifications})";
            }
            
            return format;
        }
        
        /// <summary>
        /// Creates an OrderItems from the full format string
        /// </summary>
        public static OrderItems? FromFullFormat(string orderString)
        {
            if (string.IsNullOrWhiteSpace(orderString))
                return null;
                
            var match = System.Text.RegularExpressions.Regex.Match(
                orderString, 
                @"^(\d+):([A-Z]_\d+)(?:\s*\((.*?)\))?$", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            
            if (!match.Success)
                return null;
            
            return new OrderItems
            {
                ItemAmount = int.Parse(match.Groups[1].Value),
                MenuCode = match.Groups[2].Value,
                Modifications = match.Groups[3].Success ? match.Groups[3].Value.Trim() : null
            };
        }
    }
}
