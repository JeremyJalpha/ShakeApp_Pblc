using CbTsSa_Shared.DBModels;

namespace CbTsSa_Shared.Models
{
    /// <summary>
    /// Safe user information DTO that excludes sensitive system fields.
    /// Use this when returning user data to clients or user-facing operations.
    /// </summary>
    public class UserInfoDto
    {
        public string UserId { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? UserIndicatedCell { get; set; }
        public string? SocialMedia { get; set; }
        public bool IsVerified { get; set; }
        public bool? POPIConsent { get; set; }
        public DateTime? DtTmJoined { get; set; }
        
        // Note: CellNumber is intentionally excluded for security
        
        public static UserInfoDto FromUser(ApplicationUser user)
        {
            return new UserInfoDto
            {
                UserId = user.Id,
                Email = user.Email,
                UserIndicatedCell = user.UserIndicatedCell,
                SocialMedia = user.SocialMedia,
                IsVerified = user.IsVerified,
                POPIConsent = user.POPIConsent,
                DtTmJoined = user.DtTmJoined
            };
        }
    }
}