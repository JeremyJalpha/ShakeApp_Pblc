using CbTsSa_Shared.DBModels;

namespace CommandBot.Interfaces
{
    public interface IUserService
    {
        Task<(ApplicationUser User, bool WasCreated)> GetOrCreateUserAsync(string cellNumber);
    }
}
