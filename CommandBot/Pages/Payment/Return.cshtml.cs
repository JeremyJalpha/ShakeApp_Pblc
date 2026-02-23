using CbTsSa_Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CommandBot.Pages.Payment
{
    public class ReturnModel : PageModel
    {
        private readonly IAppDbContext _db;
        private readonly ILogger<ReturnModel> _logger;

        public ReturnModel(IAppDbContext db, ILogger<ReturnModel> logger)
        {
            _db = db;
            _logger = logger;
        }

        public string? SaleId { get; set; }
        public string? PfPaymentId { get; set; }
        public string? PaymentStatus { get; set; }
        public bool IsSuccess { get; set; }

        public async Task<IActionResult> OnGetAsync(
            [FromQuery(Name = "m_payment_id")] string? saleId,
            [FromQuery(Name = "pf_payment_id")] string? pfPaymentId,
            [FromQuery(Name = "payment_status")] string? paymentStatus,
            [FromQuery(Name = "item_name")] string? itemName)
        {
            SaleId = saleId;
            PfPaymentId = pfPaymentId;
            PaymentStatus = paymentStatus;
            IsSuccess = paymentStatus?.ToUpper() == "COMPLETE";

            if (string.IsNullOrWhiteSpace(saleId))
            {
                _logger.LogWarning("Payment return received with missing m_payment_id");
                return Page();
            }

            if (!long.TryParse(saleId, out var saleIdLong))
            {
                _logger.LogWarning("Payment return received with invalid m_payment_id: {SaleId}", saleId);
                return Page();
            }

            var sale = await _db.Sales
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.SaleID == saleIdLong);

            if (sale == null)
            {
                _logger.LogWarning("Payment return for non-existent Sale {SaleId}", saleId);
                return Page();
            }

            _logger.LogInformation(
                "Payment return for Sale {SaleId}, status: {Status}, pfPaymentId: {PfId}",
                saleId, paymentStatus, pfPaymentId);

            return Page();
        }
    }
}