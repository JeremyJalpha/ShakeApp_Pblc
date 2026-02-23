using CommandBot.Helpers;

namespace CommandBot.Commands.FieldHandlers
{
    /// <summary>
    /// Handles updates to the user's social media handle
    /// </summary>
    public class SocialFieldHandler : BaseFieldUpdateHandler
    {
        public override string FieldName => "Social";

        public SocialFieldHandler(ILogger<SocialFieldHandler> logger)
            : base(logger, UserField.Social)
        {
        }

        protected override ValidationResult Validate(string newValue)
        {
            if (string.IsNullOrWhiteSpace(newValue))
                return ValidationResult.Failure("Social media handle cannot be empty.");

            if (newValue.Length > 100)
                return ValidationResult.Failure("Social media handle cannot exceed 100 characters.");

            return ValidationResult.Success();
        }
    }
}
