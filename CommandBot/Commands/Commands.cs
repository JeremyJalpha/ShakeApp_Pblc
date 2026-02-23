using CommandBot.Interfaces;
using CommandBot.Models;

namespace CommandBot.Commands
{
    public abstract class BaseCommand : ICommand
    {
        public virtual string CommandKey { get; set; } = string.Empty;
        public virtual string Description { get; set; } = string.Empty;
        public virtual bool ShowInMenu { get; set; } = true;

        public abstract Task<string> ExecuteAsync(CommandContext context, CancellationToken cancellationToken);
    }
}
