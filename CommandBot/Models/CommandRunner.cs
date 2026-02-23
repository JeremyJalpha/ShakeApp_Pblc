using CbTsSa_Shared.CbTsSaConstants;
using CbTsSa_Shared.DBModels;
using CbTsSa_Shared.Interfaces;
using CbTsSa_Shared.Models;
using CommandBot.Interfaces;
using CommandBot.Services;
using Microsoft.Extensions.Options;

namespace CommandBot.Models
{
    public class CommandRunner : ICommandRunner
    {
        private readonly IAppDbContext _db;
        private readonly BusinessContextFactory _busiFactory;
        private readonly IOptions<HostBusiness> _hostBusiness;
        private readonly JwtIssueConfig _jwtConfig;
        private readonly ConversationContextFactory _convoFactory;
        private readonly CommandParser _commandParser;
        private readonly ILogger<CommandRunner> _logger;
        private readonly IHttpClientFactory _httpFactory;

        public CommandRunner(
            IAppDbContext db,
            BusinessContextFactory busiFactory,
            IOptions<HostBusiness> hostBusiness,
            IOptions<JwtIssueConfig> jwtConfig,
            ConversationContextFactory convoFactory,
            CommandParser commandParser,
            ILogger<CommandRunner> logger,
            IHttpClientFactory httpFactory)
        {
            _db = db;
            _busiFactory = busiFactory;
            _hostBusiness = hostBusiness;
            _jwtConfig = jwtConfig.Value;
            _convoFactory = convoFactory;
            _commandParser = commandParser;
            _logger = logger;
            _httpFactory = httpFactory;
        }

        // Now includes CancellationToken propagation
        public async Task<List<ChatDispatchRequest>> ExecuteAsync(ChatUpdate chatUpdate, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                // Diagnostic: show whether IHttpClientFactory is available (no secrets)
                _logger.LogDebug("CommandRunner starting execution. IHttpClientFactory available: {HasFactory}", _httpFactory != null);

                // Create ConversationContext (with MediaHandle)
                var convo = await _convoFactory.CreateConversationContextAsync(chatUpdate);

                // Determine the cell number to use for BusinessContext.
                // Use the configured host/business cell number for lookups (WhatsApp messages are sent TO the host number).
                // Do NOT use the sender's cell number as the business lookup key.
                var businessCellNumber = _hostBusiness.Value.Cellnumber;
                if (string.IsNullOrWhiteSpace(businessCellNumber))
                {
                    _logger.LogWarning("Host business cellnumber is not configured; falling back to incoming message origin for business lookup.");
                    businessCellNumber = chatUpdate?.From?.CellNumber ?? throw new InvalidOperationException("No cell number available for business lookup.");
                }

                // Create BusinessContext based on the host/business cell number and the channel from the update
                var busi = _busiFactory.CreateBusinessContext(
                    _db,
                    _hostBusiness.Value.BaseUrl,
                    businessCellNumber,
                    chatUpdate.Channel  // Use the channel from the update
                );

                var ctx = new CommandContext(_db, convo, busi, _jwtConfig, _logger);

                // Use injected CommandParser instance
                var commands = _commandParser.ParseCommands(convo);

                var processor = new CommandProcessor(commands);

                // propagate cancellation token into command processing
                return await processor.ProcessCommandsAsync(ctx, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("CommandRunner execution cancelled for {From}", chatUpdate?.From);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CommandRunner execution failed for {From}", chatUpdate?.From);

                return new List<ChatDispatchRequest>
                {
                    new ChatDispatchRequest
                    {
                        ChatUpdate = new ChatUpdate
                        {
                            From = chatUpdate?.From ?? new ApplicationUser { CellNumber = "unknown" },
                            Channel = ChatChannelType.None,
                            Body = "An unexpected error occurred while processing your request.",
                        },
                        Tags = new Dictionary<string, string> { { "error", "true" } }
                    }
                };
            }
        }

        // Explicitly implement the interface method to resolve CS0535
        Task<List<ChatDispatchRequest>> ICommandRunner.ExecuteAsync(ChatUpdate chatUpdate, CancellationToken cancellationToken)
        {
            return ExecuteAsync(chatUpdate, cancellationToken);
        }
    }
}