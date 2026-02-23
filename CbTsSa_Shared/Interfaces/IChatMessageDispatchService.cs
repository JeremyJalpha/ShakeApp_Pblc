using CbTsSa_Shared.Models;

namespace CbTsSa_Shared.Interfaces
{
    public interface IChatMessageDispatchService
    {
        Task DispatchAsync(ChatDispatchRequest dispatch);
    }

    public interface ITelegramDispatchService : IChatMessageDispatchService { }
    public interface IWhatsAppDispatchService : IChatMessageDispatchService { }
}
