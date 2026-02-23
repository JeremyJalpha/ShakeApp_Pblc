using CommandBot.Interfaces;
using CommandBot.Models;

namespace CommandBot.Services
{
    public class CommandParser
    {
        private readonly CommandRegistry _registry;
        private readonly ILogger<CommandParser> _logger;

        public CommandParser(CommandRegistry registry, ILogger<CommandParser> logger)
        {
            _registry = registry;
            _logger = logger;
        }

        public List<ICommand> ParseCommands(ConversationContext convo)
        {
            _logger.LogInformation(
                "🔍 CommandParser.ParseCommands - Body: '{Body}', MediaHandle: '{MediaHandle}'",
                convo.MessageBody,
                convo.MediaHandle ?? "NULL"
            );

            try
            {
                var commands = _registry.ParseCommands(convo.MessageBody);

                _logger.LogInformation("📋 Parsed {Count} command(s)", commands.Count);
                foreach (var cmd in commands)
                {
                    _logger.LogInformation("  - {CommandType}", cmd.GetType().Name);
                }

                return commands;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CommandParser failed while parsing message: '{Body}'", convo.MessageBody);
                // Return empty list so processing can continue (greeting/no-op behavior)
                return new List<ICommand>();
            }
        }
    }
}