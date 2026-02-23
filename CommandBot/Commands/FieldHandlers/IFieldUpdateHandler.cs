using CommandBot.Models;

namespace CommandBot.Commands.FieldHandlers
{
    /// <summary>
    /// Interface for handling user field updates.
    /// Each field type implements this to provide validation and update logic.
    /// </summary>
    public interface IFieldUpdateHandler
    {
        /// <summary>
        /// The field name identifier (e.g., "name", "email", "consent")
        /// </summary>
        string FieldName { get; }

        /// <summary>
        /// Validate and apply the field update
        /// </summary>
        /// <param name="newValue">The new value to set</param>
        /// <param name="context">Command execution context</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success or error message</returns>
        Task<string> ProcessAsync(string newValue, CommandContext context, CancellationToken cancellationToken);
    }
}
