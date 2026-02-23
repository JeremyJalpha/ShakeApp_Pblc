using CommandBot.Attributes;
using CommandBot.Helpers;
using CommandBot.Models;

namespace CommandBot.Commands
{
    /// <summary>
    /// Initiates the driver login flow via JWT token.
    /// Command format: #driver login or #driverlogin
    /// </summary>
    [Command("driverlogin", "Driver login", showInMenu: true, groupNumber: 1, order: 4)]
    [Command("driver login", "Driver login", groupNumber: 1, order: 4)]
    public class DriverLoginCommand : BaseCommand
    {
        private readonly ILogger<DriverLoginCommand> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public DriverLoginCommand(ILogger<DriverLoginCommand> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;

            CommandKey = "driverlogin";
            Description = "Driver login";
        }

        public override async Task<string> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (context?.ConvoContext == null || context.BusiContext == null || context.JwtConfig == null)
            {
                _logger.LogError("Required context is missing for driver login");
                throw new ArgumentNullException("Required context is missing.");
            }

            try
            {
                // keep reference to factory for consistency with command pattern
                _logger.LogDebug("HttpClientFactory available for DriverLoginCommand: {HasFactory}", _httpClientFactory != null);

                // Check cancellation before starting long-running operation
                cancellationToken.ThrowIfCancellationRequested();

                var result = await CBJwtHelper.BeginDriverLoginAsync(
                    context.AppDbContext,
                    context.ConvoContext.MsgOriginNumber,
                    context.BusiContext.BaseUrl,
                    context.JwtConfig.Issuer,
                    context.JwtConfig.Key,
                    context.JwtConfig.Audiences,
                    context.JwtConfig.TokenLifetimeMinutes
                );

                cancellationToken.ThrowIfCancellationRequested();

                if (string.IsNullOrWhiteSpace(result))
                {
                    _logger.LogWarning(
                        "Driver login failed for number {PhoneNumber}",
                        context.ConvoContext.MsgOriginNumber);
                    return "❌ Driver login failed. Please contact support.";
                }

                _logger.LogInformation(
                    "Driver login initiated for {PhoneNumber}",
                    context.ConvoContext.MsgOriginNumber);

                return result;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("DriverLoginCommand cancelled for number {PhoneNumber}", context?.ConvoContext?.MsgOriginNumber);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during driver login process");
                return $"❌ Driver login failed: {ex.Message}";
            }
        }
    }
}