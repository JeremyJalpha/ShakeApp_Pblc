using CommandBot.Attributes;
using CommandBot.Interfaces;
using CommandBot.Models;
using Microsoft.Extensions.Options;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace CommandBot.Services
{
    public class CommandRegistry
    {
        private readonly Dictionary<string, (Type CommandType, int GroupNumber, int Order)> _exactMatches = new();
        private readonly List<(Regex Pattern, Type CommandType, string Description, bool ShowInMenu, string? Example, int GroupNumber, int Order)> _patternMatches = new();
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CommandRegistry> _logger;
        private readonly CommandConfiguration _config;

        public CommandRegistry(
            IServiceProvider serviceProvider,
            ILogger<CommandRegistry> logger,
            IOptions<CommandConfiguration> config)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _config = config.Value;

            RegisterCommands();
        }

        private void RegisterCommands()
        {
            var commandTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => typeof(ICommand).IsAssignableFrom(t) && !t.IsAbstract);

            foreach (var type in commandTypes)
            {
                var attributes = type.GetCustomAttributes<CommandAttribute>();

                foreach (var attr in attributes)
                {
                    // Check if command is disabled in configuration
                    if (_config.DisabledCommands.Contains(attr.Pattern))
                    {
                        _logger.LogInformation("Command {Pattern} is disabled", attr.Pattern);
                        continue;
                    }

                    if (attr.Pattern.Contains("*") || attr.Pattern.Contains("(") || attr.Pattern.Contains(@"\s"))
                    {
                        // Pattern-based command (regex)
                        var regex = new Regex(attr.Pattern, RegexOptions.IgnoreCase);
                        _patternMatches.Add((regex, type, attr.Description, attr.ShowInMenu, attr.Example, attr.GroupNumber, attr.Order));
                        _logger.LogInformation("Registered pattern command: {Pattern} -> {Type}", attr.Pattern, type.Name);
                    }
                    else
                    {
                        // Exact match command
                        _exactMatches[attr.Pattern.ToLowerInvariant()] = (type, attr.GroupNumber, attr.Order);
                        _logger.LogInformation("Registered command: {Pattern} -> {Type}", attr.Pattern, type.Name);
                    }
                }
            }

            _logger.LogInformation(
                "Command registration complete: {ExactCount} exact, {PatternCount} pattern commands",
                _exactMatches.Count,
                _patternMatches.Count);
        }

        public List<ICommand> ParseCommands(string messageBody)
        {
            var commands = new List<ICommand>();

            // First try exact matches
            foreach (var kvp in _exactMatches)
            {
                if (messageBody.Contains($"#{kvp.Key}", StringComparison.OrdinalIgnoreCase))
                {
                    var command = CreateCommand(kvp.Value.CommandType);
                    if (command != null)
                    {
                        command.CommandKey = kvp.Key;
                        commands.Add(command);
                    }
                }
            }

            // Then try pattern matches
            foreach (var (pattern, commandType, description, showInMenu, example, groupNumber, order) in _patternMatches)
            {
                try
                {
                var match = pattern.Match(messageBody);
                if (match.Success)
                {
                    var command = CreateCommand(commandType);
                    if (command != null)
                    {
                        // Pass match groups to command for parameter extraction
                        if (command is IPatternCommand patternCommand)
                        {
                            patternCommand.Initialize(match);
                        }
                        commands.Add(command);
                    }
                        else
                        {
                            _logger.LogError("❌ CreateCommand returned null for type {Type}", commandType.Name);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to evaluate pattern '{Pattern}'", pattern.ToString());
                }
            }

            return commands;
        }

        private ICommand? CreateCommand(Type commandType)
        {
            try
            {
                var instance = (ICommand?)ActivatorUtilities.CreateInstance(_serviceProvider, commandType);
                if (instance == null)
                {
                    _logger.LogError("❌ ActivatorUtilities.CreateInstance returned null for {Type}", commandType.Name);
                }
                return instance;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ EXCEPTION: Failed to create command of type {Type}. Exception: {Message}", commandType.Name, ex.Message);
                return null;
            }
        }

        public string GenerateMenu()
        {
            var sb = new StringBuilder("📋 Main Menu:\n\n");

            // Collect all commands with their display info
            var allCommands = new List<(string DisplayText, int GroupNumber, int Order)>();

            // Add exact match commands that should be shown
            foreach (var kvp in _exactMatches)
            {
                var attrs = kvp.Value.CommandType.GetCustomAttributes<CommandAttribute>();
                var matchingAttr = attrs.FirstOrDefault(a =>
                    a.Pattern.Equals(kvp.Key, StringComparison.OrdinalIgnoreCase));

                if (matchingAttr?.ShowInMenu == true)
                {
                    allCommands.Add((
                        $"#{kvp.Key} - {matchingAttr.Description}",
                        kvp.Value.GroupNumber,
                        kvp.Value.Order
                    ));
                }
            }

            // Add pattern commands that should be shown
            foreach (var pm in _patternMatches.Where(pm => pm.ShowInMenu))
            {
                // Use Description only if Example is null, otherwise use Example with Description and separator
                var displayText = string.IsNullOrEmpty(pm.Example)
                    ? pm.Description
                    : $"{pm.Example} - {pm.Description}";
                    
                allCommands.Add((
                    displayText,
                    pm.GroupNumber,
                    pm.Order
                ));
            }

            // Group and order commands
            var groupedCommands = allCommands
                .OrderBy(c => c.GroupNumber)
                .ThenBy(c => c.Order)
                .ThenBy(c => c.DisplayText)
                .GroupBy(c => c.GroupNumber);

            bool firstGroup = true;
            foreach (var group in groupedCommands)
            {
                // Add spacing between groups (except before the first group)
                if (!firstGroup)
                {
                    sb.AppendLine();
                }
                firstGroup = false;

                foreach (var command in group)
                {
                    sb.AppendLine(command.DisplayText);
                }
            }

            return sb.ToString();
        }
    }
}
