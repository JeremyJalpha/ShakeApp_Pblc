using System.Text.RegularExpressions;
using CommandBot.Commands.FieldHandlers;
using CommandBot.Interfaces;
using CommandBot.Models;
using CommandBot.Attributes;

namespace CommandBot.Commands
{
    /// <summary>
    /// Handles user field updates using a registry pattern.
    /// Command format: #update {fieldname}: {value}
    /// 
    /// Examples:
    /// - #update name: John Doe
    /// - #update email: john@example.com
    /// - #update consent: yes
    /// 
    /// Field types are dynamically registered via FieldUpdateHandlerRegistry.
    /// </summary>
    // Prevent matching the 'order' field here so specialized UpdateOrderCommand handles it.
    [Command(@"#update\s+(?!order\b)(\w+):\s*(.+)",
             "#update name: <name>\n#update email: <email>\n#update social: <social>\n#update cell: <cell>\n#update consent: <yes/no>",
             showInMenu: true,
             groupNumber: 3,
             order: 2)]
    public class UpdateUserFieldCommand : BaseCommand, IPatternCommand
    {
        private string _fieldName = string.Empty;
        private string _newValue = string.Empty;
        private readonly ILogger<UpdateUserFieldCommand> _logger;
        private readonly FieldUpdateHandlerRegistry _registry;

        public UpdateUserFieldCommand(
            ILogger<UpdateUserFieldCommand> logger,
            FieldUpdateHandlerRegistry registry)
        {
            _logger = logger;
            _registry = registry;
            CommandKey = "update";
            Description = "Update a user field";
        }

        public void Initialize(Match match)
        {
            _fieldName = match.Groups[1].Value.Trim();
            _newValue = match.Groups[2].Value.Trim();
            CommandKey = $"update:{_fieldName}";
            Description = $"Update {_fieldName}";
        }

        public override async Task<string> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
        {
            if (context?.ConvoContext?.User == null)
                return "❌ User context is missing.";

            try
            {
                _logger.LogDebug("Processing field update: field={FieldName}", _fieldName);

                var handler = _registry.GetHandler(_fieldName);
                if (handler == null)
                {
                    var availableFields = string.Join(", ", _registry.GetRegisteredFieldNames());
                    return $"❌ Unknown field '{_fieldName}'.\n\nAvailable fields: {availableFields}";
                }

                return await handler.ProcessAsync(_newValue, context, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("UpdateUserFieldCommand cancelled by token");
                throw;
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid value for {FieldName}", _fieldName);
                return $"❌ Invalid value: {ex.Message}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update {FieldName}", _fieldName);
                return $"❌ Failed to update {_fieldName}: {ex.Message}";
            }
        }
    }
}