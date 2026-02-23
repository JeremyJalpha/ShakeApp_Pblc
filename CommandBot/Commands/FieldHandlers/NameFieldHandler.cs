using CommandBot.Helpers;

namespace CommandBot.Commands.FieldHandlers
{
    /// <summary>
    /// Handles updates to the user's name (UserName property)
    /// </summary>
    public class NameFieldHandler : BaseFieldUpdateHandler
    {
        public override string FieldName => "Name";

        public NameFieldHandler(ILogger<NameFieldHandler> logger)
            : base(logger, UserField.Name)
        {
        }

        protected override ValidationResult Validate(string newValue)
        {
            if (string.IsNullOrWhiteSpace(newValue))
                return ValidationResult.Failure("Name cannot be empty.");

            if (newValue.Length < 2)
                return ValidationResult.Failure("Name must be at least 2 characters.");

            if (newValue.Length > 100)
                return ValidationResult.Failure("Name cannot exceed 100 characters.");

            return ValidationResult.Success();
        }
    }
}
