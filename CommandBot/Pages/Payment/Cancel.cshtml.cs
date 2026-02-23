using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CommandBot.Pages.Payment
{
    public class CancelModel : PageModel
    {
        private readonly ILogger<CancelModel> _logger;

        public CancelModel(ILogger<CancelModel> logger)
        {
            _logger = logger;
        }

        public string? SaleId { get; set; }

        public IActionResult OnGet([FromQuery(Name = "m_payment_id")] string? saleId)
        {
            SaleId = saleId;

            if (!string.IsNullOrWhiteSpace(saleId))
            {
                _logger.LogWarning("Payment cancelled for Sale {SaleId}", saleId);
            }
            else
            {
                _logger.LogWarning("Payment cancelled (no Sale ID provided)");
            }

            return Page();
        }
    }
}