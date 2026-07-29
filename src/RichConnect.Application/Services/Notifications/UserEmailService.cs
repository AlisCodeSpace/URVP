using Microsoft.EntityFrameworkCore;
using RICHConnect.Backend.Application.Interfaces.Notifications;
using RICHConnect.Backend.Infrastructure.Data;

namespace RICHConnect.Backend.Application.Services.Notifications
{
    public class UserEmailService : IUserEmailService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UserEmailService> _logger;
        
        public UserEmailService(AppDbContext context, ILogger<UserEmailService> logger)
        {
            _context = context;
            _logger = logger;
        }
        
        public async Task<string?> GetUserEmailAsync(Guid userId)
        {
            try
            {
                var user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == userId);
                return user?.Email;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving email for user {UserId}", userId);
                return null;
            }
        }
        
        public async Task<string?> GetUserNameAsync(Guid userId)
        {
            try
            {
                var user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == userId);
                return user?.Name;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving name for user {UserId}", userId);
                return null;
            }
        }
    }
}
