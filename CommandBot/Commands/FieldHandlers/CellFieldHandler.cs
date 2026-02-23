using System.Text.RegularExpressions;
using CommandBot.Helpers;

namespace CommandBot.Commands.FieldHandlers
{
    /// <summary>
    /// Handles updates to the user's indicated cell number (UserIndicatedCell property)
    /// </summary>
    public class CellFieldHandler : BaseFieldUpdateHandler
    {
        private static readonly Regex _digitsOnlyRegex = new(@"[\s\-\(\)\+]", RegexOptions.Compiled);

        public override string FieldName => "Cell";

        public CellFieldHandler(ILogger<CellFieldHandler> logger)
            : base(logger, UserField.Cell)
        {
        }

        protected override ValidationResult Validate(string newValue)
        {
            if (string.IsNullOrWhiteSpace(newValue))
                return ValidationResult.Failure("Cell number cannot be empty.");

            // Remove common formatting characters for validation
            var digitsOnly = _digitsOnlyRegex.Replace(newValue, "");

            if (digitsOnly.Length < 10)
                return ValidationResult.Failure("Cell number must have at least 10 digits.");

            if (digitsOnly.Length > 15)
                return ValidationResult.Failure("Cell number cannot exceed 15 digits.");

            return ValidationResult.Success();
        }
    }
}
