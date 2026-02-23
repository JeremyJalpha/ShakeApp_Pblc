using CommandBot.Interfaces;
using CbTsSa_Shared.DBModels;
using CbTsSa_Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CommandBot.Services
{
    public class UserService : IUserService
    {
        private readonly IAppDbContext _dbContext;
        private readonly ILogger<UserService> _logger;

        public UserService(IAppDbContext dbContext, ILogger<UserService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<(ApplicationUser User, bool WasCreated)> GetOrCreateUserAsync(string cellNumber)
        {
            if (string.IsNullOrWhiteSpace(cellNumber))
            {
                throw new ArgumentException("Cell number cannot be null or empty.", nameof(cellNumber));
            }

            if (!long.TryParse(cellNumber, out long parsedCellNumber))
            {
                throw new ArgumentException("Cell number must be a valid integer.", nameof(cellNumber));
            }

            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.CellNumber == cellNumber);

            if (user != null)
            {
                return (user, false); // User already existed
            }

            // Create new user
            _logger.LogInformation("User with phone number {CellNumber} does not exist. Creating a new user.", cellNumber);

            var newUser = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                CellNumber = cellNumber,
                UserIndicatedCell = cellNumber,
                SecurityStamp = Guid.NewGuid().ToString(),
                DtTmJoined = DateTime.UtcNow
            };

            _dbContext.Users.Add(newUser);
            await _dbContext.SaveChangesAsync();

            return (newUser, true); // User was just created
        }
    }
}
