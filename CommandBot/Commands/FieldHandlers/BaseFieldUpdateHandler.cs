using CommandBot.Helpers;
using CommandBot.Models;
using System.Reflection;

namespace CommandBot.Commands.FieldHandlers
{
    /// <summary>
    /// Base class for field update handlers that use UserPropertyMapper.
    /// Handles common validation and persistence logic.
    /// </summary>
    public abstract class BaseFieldUpdateHandler : IFieldUpdateHandler
    {
        protected readonly ILogger _logger;
        protected readonly UserField _userField;
        protected readonly PropertyInfo _propertyInfo;

        public abstract string FieldName { get; }

        protected BaseFieldUpdateHandler(ILogger logger, UserField userField)
        {
            _logger = logger;
            _userField = userField;
            _propertyInfo = UserPropertyMapper.GetPropertyInfo(userField);
        }

        public async Task<string> ProcessAsync(string newValue, CommandContext context, CancellationToken cancellationToken)
        {
            var user = context.ConvoContext?.User;
            if (user == null)
                return "❌ User not found.";

            try
            {
                // Validate the new value
                var validationResult = Validate(newValue);
                if (!validationResult.IsValid)
                    return $"❌ {validationResult.ErrorMessage}";

                // Convert the value to the target type
                var convertedValue = ConvertValue(newValue);

                // Set the value
                _propertyInfo.SetValue(user, convertedValue);

                // Persist change
                await context.AppDbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "User {UserId} updated {Field} to {Value}",
                    user.Id,
                    FieldName,
                    newValue);

                return $"✓ Updated {FieldName} successfully.";
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Field update cancelled for {Field}", FieldName);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update field {Field}", FieldName);
                return $"❌ Failed to update {FieldName}: {ex.Message}";
            }
        }

        /// <summary>
        /// Validate the new value. Override in derived classes for custom validation.
        /// </summary>
        protected virtual ValidationResult Validate(string newValue)
        {
            if (string.IsNullOrWhiteSpace(newValue))
                return ValidationResult.Failure("Value cannot be empty.");

            return ValidationResult.Success();
        }

        /// <summary>
        /// Convert the string value to the target type. Override for custom conversion.
        /// </summary>
        protected virtual object? ConvertValue(string newValue) => newValue;

        protected record ValidationResult(bool IsValid, string? ErrorMessage = null)
        {
            public static ValidationResult Success() => new(true);
            public static ValidationResult Failure(string message) => new(false, message);
        }
    }
}
