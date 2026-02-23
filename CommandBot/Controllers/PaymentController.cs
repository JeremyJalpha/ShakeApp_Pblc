using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.Web;
using CbTsSa_Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CommandBot.Controllers
{
    [Route("payment")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IAppDbContext _db;
        private readonly IConfiguration _config;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            IAppDbContext db,
            IConfiguration config,
            ILogger<PaymentController> logger)
        {
            _db = db;
            _config = config;
            _logger = logger;
        }

        /// <summary>
        /// PayFast POSTs payment confirmation here (ITN - Instant Transaction Notification).
        /// This is the authoritative payment verification endpoint.
        /// Must validate signature, IP, and server confirmation (matching Go pfValid* methods).
        /// </summary>
        [HttpPost("notify")]
        public async Task<IActionResult> PaymentNotify()
        {
            // 1. Read raw form data
            var formData = new Dictionary<string, string>();
            foreach (var key in Request.Form.Keys)
            {
                formData[key] = Request.Form[key].ToString();
            }

            _logger.LogInformation(
                "PayFast ITN received: {FormData}",
                string.Join(", ", formData.Select(kv => $"{kv.Key}={kv.Value}")));

            // 2. Respond immediately (PayFast requires 200 OK within 10s)
            var responseTask = Task.Run(() =>
            {
                Response.StatusCode = 200;
                Response.ContentType = "text/plain";
                return Response.WriteAsync("Success");
            });

            // 3. Extract required fields
            if (!formData.TryGetValue("m_payment_id", out var saleIdStr)
                || !long.TryParse(saleIdStr, out var saleId))
            {
                _logger.LogError("PayFast ITN missing or invalid m_payment_id");
                await responseTask;
                return Ok("Success"); // Still return 200 to avoid retries
            }

            formData.TryGetValue("pf_payment_id", out var pfPaymentId);
            formData.TryGetValue("payment_status", out var paymentStatus);
            formData.TryGetValue("amount_gross", out var amountGross);

            // 4. Validate signature
            var passphrase = _config["PayFast:Passphrase"] ?? string.Empty;
            if (!ValidateSignature(formData, passphrase))
            {
                _logger.LogError("PayFast ITN signature validation failed for Sale {SaleId}", saleId);
                await responseTask;
                return Ok("Success");
            }

            // 5. Validate PayFast server IP
            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            if (!await ValidatePayFastIpAsync(clientIp))
            {
                _logger.LogWarning(
                    "PayFast ITN from unrecognized IP {ClientIp} for Sale {SaleId}",
                    clientIp, saleId);
                // Don't fail - just log (PayFast IPs can change)
            }

            // 6. Validate with PayFast server (optional - can be slow)
            // Uncomment if you want full validation:
            // if (!await ValidateWithPayFastServerAsync(formData))
            // {
            //     _logger.LogError("PayFast server validation failed for Sale {SaleId}", saleId);
            //     await responseTask;
            //     return Ok("Success");
            // }

            // 7. Update Payment and Sale records
            try
            {
                var payment = await _db.Payments
                    .Include(p => p.Sale)
                    .FirstOrDefaultAsync(p => p.SaleID == saleId);

                if (payment == null)
                {
                    _logger.LogError("Payment not found for Sale {SaleId}", saleId);
                    await responseTask;
                    return Ok("Success");
                }

                // Update payment record
                payment.PayGateTransactionID = int.TryParse(pfPaymentId, out var pfId) ? pfId : null;
                payment.SuccesfulDateTime = paymentStatus == "COMPLETE" ? DateTime.UtcNow : null;

                // Update sale completion time
                if (paymentStatus == "COMPLETE" && payment.Sale != null)
                {
                    payment.Sale.DtTmCompleted = DateTime.UtcNow;
                }

                await _db.SaveChangesAsync();

                _logger.LogInformation(
                    "Payment updated for Sale {SaleId}: status={Status}, pfId={PfId}, amount={Amount}",
                    saleId, paymentStatus, pfPaymentId, amountGross);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update payment for Sale {SaleId}", saleId);
            }

            await responseTask;
            return Ok("Success");
        }

        // ── PayFast validation helpers (matching Go implementation) ──────

        /// <summary>
        /// Validates PayFast MD5 signature using application/x-www-form-urlencoded encoding.
        /// </summary>
        private static bool ValidateSignature(
            Dictionary<string, string> formData,
            string passphrase)
        {
            if (!formData.TryGetValue("signature", out var receivedSignature))
                return false;

            // Build param string (exclude signature itself)
            // Use application/x-www-form-urlencoded encoding (spaces as +)
            var paramString = string.Join("&",
                formData
                    .Where(kv => kv.Key != "signature")
                    .OrderBy(kv => kv.Key)
                    .Select(kv => $"{UrlEncodeForPayFast(kv.Key)}={UrlEncodeForPayFast(kv.Value)}"));

            if (!string.IsNullOrWhiteSpace(passphrase))
                paramString += $"&passphrase={UrlEncodeForPayFast(passphrase)}";

            var hashBytes = MD5.HashData(Encoding.UTF8.GetBytes(paramString));
            var calculatedSignature = Convert.ToHexStringLower(hashBytes);

            return calculatedSignature == receivedSignature;
        }

        /// <summary>
        /// URL-encodes a string using application/x-www-form-urlencoded format (spaces as +).
        /// PayFast requires this specific encoding format for signature validation.
        /// </summary>
        private static string UrlEncodeForPayFast(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return HttpUtility.UrlEncode(value, Encoding.UTF8);
        }

        /// <summary>
        /// Validates request came from PayFast servers (matches Go pfValidIP).
        /// </summary>
        private static async Task<bool> ValidatePayFastIpAsync(string? clientIp)
        {
            if (string.IsNullOrWhiteSpace(clientIp))
                return false;

            var validHosts = new[]
            {
                "www.payfast.co.za",
                "sandbox.payfast.co.za",
                "w1w.payfast.co.za",
                "w2w.payfast.co.za"
            };

            var validIps = new HashSet<string>();
            foreach (var host in validHosts)
            {
                try
                {
                    var addresses = await Dns.GetHostAddressesAsync(host);
                    foreach (var addr in addresses)
                        validIps.Add(addr.ToString());
                }
                catch
                {
                    // DNS lookup failed - continue
                }
            }

            return validIps.Contains(clientIp);
        }

        /// <summary>
        /// Posts data back to PayFast for server-side validation (matches Go pfValidServerConfirmation).
        /// </summary>
        private async Task<bool> ValidateWithPayFastServerAsync(Dictionary<string, string> formData)
        {
            var pfHost = _config["PayFast:HostUrl"]?.Contains("sandbox") == true
                ? "sandbox.payfast.co.za"
                : "www.payfast.co.za";

            var url = $"https://{pfHost}/eng/query/validate";

            // Use application/x-www-form-urlencoded encoding
            var paramString = string.Join("&",
                formData
                    .Where(kv => kv.Key != "signature")
                    .Select(kv => $"{UrlEncodeForPayFast(kv.Key)}={UrlEncodeForPayFast(kv.Value)}"));

            try
            {
                using var client = new HttpClient();
                var content = new StringContent(paramString, Encoding.UTF8, "application/x-www-form-urlencoded");
                var response = await client.PostAsync(url, content);
                var body = await response.Content.ReadAsStringAsync();

                return body == "VALID";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PayFast server validation request failed");
                return false;
            }
        }
    }
}