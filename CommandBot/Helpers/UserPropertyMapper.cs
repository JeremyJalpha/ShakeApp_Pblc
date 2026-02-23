using CbTsSa_Shared.DBModels;
using System.Reflection;

namespace CommandBot.Helpers
{
    public enum UserField
    {
        Name,
        Social,
        Cell,
        Consent,
        Verified,
        Email
    }

    public static class UserPropertyMapper
    {
        // ✅ Cached reflection metadata (computed once at startup) - faster than a dictionary lookup
        private static readonly PropertyInfo _socialProperty = typeof(ApplicationUser).GetProperty(nameof(ApplicationUser.SocialMedia))!;
        private static readonly PropertyInfo _cellProperty = typeof(ApplicationUser).GetProperty(nameof(ApplicationUser.UserIndicatedCell))!;
        private static readonly PropertyInfo _consentProperty = typeof(ApplicationUser).GetProperty(nameof(ApplicationUser.POPIConsent))!;
        private static readonly PropertyInfo _verifiedProperty = typeof(ApplicationUser).GetProperty(nameof(ApplicationUser.IsVerified))!;
        private static readonly PropertyInfo _emailProperty = typeof(ApplicationUser).GetProperty(nameof(ApplicationUser.Email))!;
        private static readonly PropertyInfo _nameProperty = typeof(ApplicationUser).GetProperty(nameof(ApplicationUser.UserName))!;

        // ✅ Fastest possible lookup using switch (compiler-optimized jump table)
        public static PropertyInfo GetPropertyInfo(UserField field)
        {
            return field switch
            {
                UserField.Name => _nameProperty,
                UserField.Social => _socialProperty,
                UserField.Cell => _cellProperty,
                UserField.Consent => _consentProperty,
                UserField.Verified => _verifiedProperty,
                UserField.Email => _emailProperty,
                _ => throw new ArgumentOutOfRangeException(nameof(field), field, "Unknown user property")
            };
        }

        // Helper to parse user input to UserField enum
        public static bool TryParseUserProperty(string input, out UserField field)
        {
            return Enum.TryParse(input, true, out field);
        }
    }
}