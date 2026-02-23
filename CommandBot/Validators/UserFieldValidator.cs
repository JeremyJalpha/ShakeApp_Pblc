using CommandBot.Helpers;

namespace CommandBot.Validators
{
    public static class UserFieldValidator
    {
        private static readonly HashSet<UserField> _userAccessibleFields = new()
        {
            UserField.Social,
            UserField.Cell,  // Maps to UserIndicatedCell
            UserField.Consent,
            UserField.Email,
            UserField.Verified  // Users can READ this, but cannot UPDATE it
        };

        /// <summary>
        /// Checks if a user can view/access this field (read-only or read-write).
        /// </summary>
        public static bool IsUserAccessible(UserField field)
        {
            return _userAccessibleFields.Contains(field);
        }

        /// <summary>
        /// Checks if a user can update/modify this field (write permission).
        /// </summary>
        public static bool CanUserUpdate(UserField field)
        {
            // Verified can only be set by the system
            return field != UserField.Verified;
        }
    }
}