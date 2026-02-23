using CbTsSa_Shared.CbTsSaConstants;
using CbTsSa_Shared.DBModels;
using CbTsSa_Shared.Models;
using CommandBot.Interfaces;

namespace CommandBot.Models
{
    public class CommandProcessor
    {
        private readonly List<ICommand> _commands;

        public CommandProcessor(List<ICommand> commands)
        {
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        public async Task<List<ChatDispatchRequest>> ProcessCommandsAsync(CommandContext ctx, CancellationToken cancellationToken)
        {
            var dispatches = new List<ChatDispatchRequest>();

            // Check if this is a new user who needs to be greeted
            if (ctx.ConvoContext.UserWasCreated)
            {
                // If there are commands to process, use warm greeting; otherwise use cold greeting
                var greetingMessage = _commands.Count > 0
                    ? (!string.IsNullOrWhiteSpace(ctx.BusiContext.NewUserGreeting_Warm)
                        ? ctx.BusiContext.NewUserGreeting_Warm
                        : CbTsSaConstants.DefaultGreeting_Warm)
                    : (!string.IsNullOrWhiteSpace(ctx.BusiContext.NewUserGreeting_Cold)
                        ? ctx.BusiContext.NewUserGreeting_Cold
                        : CbTsSaConstants.DefaultGreeting_Cold);

                dispatches.Add(new ChatDispatchRequest
                {
                    ChatUpdate = new ChatUpdate
                    {
                        From = new ApplicationUser { CellNumber = ctx.ConvoContext.User.CellNumber },
                        Body = greetingMessage,
                        Channel = ctx.ConvoContext.Channel
                    },
                    Tags = new Dictionary<string, string>
                    {
                        { "type", "greeting" },
                        { "greeting_type", _commands.Count > 0 ? "warm" : "cold" },
                        { "status", "ok" }
                    }
                });
            }

            if (_commands.Count == 0)
            {
                dispatches.Add(new ChatDispatchRequest
                {
                    ChatUpdate = new ChatUpdate
                    {
                        From = new ApplicationUser { CellNumber = ctx.ConvoContext.User.CellNumber },
                        Body = CbTsSaConstants.NoCommandsFoundInMessage + " " + CbTsSaConstants.MainMenuHint,
                        Channel = ctx.ConvoContext.Channel
                    },
                    Tags = new Dictionary<string, string>
                    {
                        { "status", "noop" }
                    }
                });
                return dispatches;
            }

            foreach (var command in _commands)
            {
                var commandName = command.CommandKey;

                try
                {
                    // 🔧 Store the incoming MediaHandle (for commands that need it like CampaignImageCommand)
                    var incomingMediaHandle = ctx.ConvoContext.MediaHandle;

                    // propagate cancellation token into command execution
                    var result = await command.ExecuteAsync(ctx, cancellationToken);

                    // 🔧 Check if command set a NEW MediaHandle for response (like ShowCurrentImageCommand)
                    var outgoingMediaHandle = ctx.ConvoContext.MediaHandle;

                    // If MediaHandle changed, command wants to send media
                    var hasResponseMedia = !string.IsNullOrWhiteSpace(outgoingMediaHandle) &&
                                           outgoingMediaHandle != incomingMediaHandle;

                    // Create the response ChatUpdate
                    var responseUpdate = new ChatUpdate
                    {
                        From = new ApplicationUser { CellNumber = ctx.ConvoContext.User.CellNumber },
                        Body = result,
                        Channel = ctx.ConvoContext.Channel,
                        MediaHandle = hasResponseMedia ? outgoingMediaHandle : null,  // Use response media if set
                        MessageType = hasResponseMedia ? ChatMessageType.Image : ChatMessageType.Text
                    };

                    dispatches.Add(new ChatDispatchRequest
                    {
                        ChatUpdate = responseUpdate,
                        Tags = new Dictionary<string, string>
                        {
                            { "command", commandName },
                            { "status", "ok" }
                        }
                    });
                }
                catch (Exception ex)
                {
                    ctx.Logger.LogError(ex, "Failed to execute command {CommandName}", commandName);

                    dispatches.Add(new ChatDispatchRequest
                    {
                        ChatUpdate = new ChatUpdate
                        {
                            From = new ApplicationUser { CellNumber = ctx.ConvoContext.User.CellNumber },
                            Body = $"❌ Failed to execute `{commandName}`",
                            Channel = ctx.ConvoContext.Channel
                        },
                        Tags = new Dictionary<string, string>
                        {
                            { "command", commandName },
                            { "status", "error" },
                            { "exception", ex.GetType().Name }
                        }
                    });
                }
            }

            return dispatches;
        }
    }
}
