using CommandBot.Helpers;

namespace CommandBot.Commands.FieldHandlers
{
    /// <summary>
    /// Handles updates to the user's POPI consent flag
    /// </summary>
    public class ConsentFieldHandler : BaseFieldUpdateHandler
    {
        public override string FieldName => "Consent";

        public ConsentFieldHandler(ILogger<ConsentFieldHandler> logger)
            : base(logger, UserField.Consent)
        {
        }

        protected override ValidationResult Validate(string newValue)
        {
            if (string.IsNullOrWhiteSpace(newValue))
                return ValidationResult.Failure("Consent value cannot be empty. Use 'yes' or 'no'.");

            var normalized = newValue.ToLowerInvariant();
            if (normalized is not ("yes" or "true" or "1" or "no" or "false" or "0"))
                return ValidationResult.Failure("Invalid consent value. Use 'yes' or 'no'.");

            return ValidationResult.Success();
        }

        protected override object? ConvertValue(string newValue)
        {
            return newValue.ToLowerInvariant() switch
            {
                "yes" or "true" or "1" => true,
                "no" or "false" or "0" => false,
                _ => throw new ArgumentException("Invalid boolean value")
            };
        }
    }
}
