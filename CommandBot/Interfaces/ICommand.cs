using CommandBot.Models;

namespace CommandBot.Interfaces
{
    public interface ICommand
    {
        /// <summary>
        /// The unique identifier for this command (e.g., "menu", "shop", "update:email")
        /// </summary>
        string CommandKey { get; set; }

        /// <summary>
        /// Human-readable description for help text and menu generation
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Whether this command should appear in the main menu
        /// </summary>
        bool ShowInMenu { get; set; }

        /// <summary>
        /// Execute the command with the given context and support cooperative cancellation.
        /// </summary>
        Task<string> ExecuteAsync(CommandContext context, CancellationToken cancellationToken);
    }
}
