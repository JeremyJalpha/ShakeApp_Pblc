using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CbTsSa_Shared.Security;

public static class JwtHelperUtil
{
    public static string GenerateJwt(
        string userId,
        string phoneNumber,
        string role,
        string issuer,
        string signingKey,
        List<string> audiences,
        int tokenMinutes)
    {
        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(tokenMinutes);
        
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.Iss, issuer),
            new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(expires).ToUnixTimeSeconds().ToString()),
            new Claim("phone_number", phoneNumber),
            new Claim("role", role)
        };

        // Manually inject multiple audiences as separate claims
        claims.AddRange(audiences.Select(aud => new Claim(JwtRegisteredClaimNames.Aud, aud)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static IEnumerable<Claim> ExtractClaims(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return Enumerable.Empty<Claim>();

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            return jwt.Claims;
        }
        catch
        {
            return Enumerable.Empty<Claim>();
        }
    }

    public static string? ExtractClaim(string token, string claimType)
    {
        return ExtractClaims(token)
            .FirstOrDefault(c => c.Type == claimType)
            ?.Value;
    }

    public static bool ClientSideJWTPrevalidation(
        string token,
        List<string> expectedAudiences,
        string expectedIssuer = CbTsSaConstants.CbTsSaConstants.ExpectedIssuer)
    {
        var handler = new JwtSecurityTokenHandler();

        if (string.IsNullOrWhiteSpace(token) || expectedAudiences == null || expectedAudiences.Count == 0)
            return false;

        // Check if the token is well-formed
        if (!handler.CanReadToken(token))
            return false;
        if (string.IsNullOrWhiteSpace(expectedIssuer))
            expectedIssuer = CbTsSaConstants.CbTsSaConstants.ExpectedIssuer;

        try
        {
            var jwtToken = handler.ReadJwtToken(token);

            // Expiry check
            var expClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "exp");
            if (expClaim == null || !long.TryParse(expClaim.Value, out var expUnix))
                return false;

            var expDate = DateTimeOffset.FromUnixTimeSeconds(expUnix);
            if (expDate <= DateTimeOffset.UtcNow)
                return false;

            // Issuer sanity check
            if (!string.Equals(jwtToken.Issuer, expectedIssuer, StringComparison.OrdinalIgnoreCase))
                return false;

            // Audience check (check all audiences in the token)
            var audClaims = jwtToken.Audiences;
            if (!audClaims.Any() || !audClaims.Any(aud => expectedAudiences.Any(ea => string.Equals(aud, ea, StringComparison.OrdinalIgnoreCase))))
                return false;

            return true;
        }
        catch
        {
            return false;
        }
    }
}