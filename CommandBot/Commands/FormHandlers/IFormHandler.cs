using CommandBot.Models;

namespace CommandBot.Commands.FormHandlers
{
    /// <summary>
    /// Interface for handling different web form types.
    /// Each form type implements this to provide parsing and processing logic.
    /// </summary>
    public interface IFormHandler
    {
        /// <summary>
        /// The form type identifier (e.g., "usersignup", "foodpoisoning")
        /// </summary>
        string FormType { get; }

        /// <summary>
        /// Parse the form data and save to database
        /// </summary>
        /// <param name="formData">Raw form data after the colon</param>
        /// <param name="context">Command execution context</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success message with formatted details</returns>
        Task<string> ProcessAsync(string formData, CommandContext context, CancellationToken cancellationToken);
    }
}
