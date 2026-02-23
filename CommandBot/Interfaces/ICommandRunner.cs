using CbTsSa_Shared.Models;

namespace CommandBot.Interfaces
{
    public interface ICommandRunner
    {
        Task<List<ChatDispatchRequest>> ExecuteAsync(ChatUpdate chatUpdate, CancellationToken cancellationToken);
    }
}
