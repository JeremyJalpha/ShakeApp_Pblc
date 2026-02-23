using System.Text.RegularExpressions;
using CommandBot.Helpers;

namespace CommandBot.Commands.FieldHandlers
{
    /// <summary>
    /// Handles updates to the user's email address
    /// </summary>
    public class EmailFieldHandler : BaseFieldUpdateHandler
    {
        private static readonly Regex _emailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public override string FieldName => "Email";

        public EmailFieldHandler(ILogger<EmailFieldHandler> logger)
            : base(logger, UserField.Email)
        {
        }

        protected override ValidationResult Validate(string newValue)
        {
            if (string.IsNullOrWhiteSpace(newValue))
                return ValidationResult.Failure("Email cannot be empty.");

            if (!_emailRegex.IsMatch(newValue))
                return ValidationResult.Failure("Invalid email format.");

            return ValidationResult.Success();
        }
    }
}
