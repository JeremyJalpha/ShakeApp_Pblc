using CbTsSa_Shared.Interfaces;
using CbTsSa_Shared.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CommandBot.Helpers
{
    public static class CBJwtHelper
    {
        private static readonly ILogger _logger = LoggerFactory.Create(lb => lb.AddConsole()).CreateLogger("CBJwtHelper");
        public static async Task<string?> FetchUserIdAsync(IAppDbContext db, string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber)) return null;

            return await db.Users
                .Where(u => u.CellNumber == phoneNumber)
                .Select(u => u.Id)
                .FirstOrDefaultAsync();
        }

        public static async Task<string?> GetUserRoleAsync(IAppDbContext db, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) return null;

            return await (from ur in db.UserRoles
                          join r in db.Roles on ur.RoleId equals r.Id
                          where ur.UserId == userId
                          select r.Name).FirstOrDefaultAsync();
        }

        public static async Task<(string? jwt, string? error)> GenerateJwtAsync(
            IAppDbContext db,
            string userPhoneNumber,
            string issuer,
            string signingKey,
            List<string> audiences,
            int tokenMinutes)
        {
            if (string.IsNullOrWhiteSpace(issuer) ||
                string.IsNullOrWhiteSpace(signingKey) ||
                audiences is null || !audiences.Any())
                return (null, "Missing JWT config - 500");

            var userId = await FetchUserIdAsync(db, userPhoneNumber);
            if (userId is null)
                return (null, "User ID not found - 404");

            var userRole = await GetUserRoleAsync(db, userId);
            if (userRole is null)
                return (null, "User role not found - 401");

            try
            {
                var jwt = JwtHelperUtil.GenerateJwt(
                    userId,
                    userPhoneNumber,
                    userRole,
                    issuer,
                    signingKey,
                    audiences,
                    tokenMinutes);

                return (jwt, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token generation failed: {Message}", ex.Message);
                return (null, "Unexpected error occurred while generating JWT - 500");
            }
        }

        public static async Task<string> BeginDriverLoginAsync(
            IAppDbContext db,
            string phoneNumber,
            string baseUrl,
            string issuer,
            string signingKey,
            List<string> audience,
            int tokenLifetimeMinutes)
        {
            var (jwt, err) = await GenerateJwtAsync(db, phoneNumber, issuer, signingKey, audience, tokenLifetimeMinutes);
            if (err != null)
                return $"OTP generation failed: {jwt} {err}";

            if (string.IsNullOrWhiteSpace(baseUrl))
                return "Base URL is empty";

            var query = new QueryString($"?jwt={Uri.EscapeDataString(jwt!)}&action=view&type=deep");
            return baseUrl + query;
        }
    }
}
