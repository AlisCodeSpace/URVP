namespace RICHConnect.Backend.Application.Interfaces.Notifications
{
    public interface IUserEmailService
    {
        Task<string?> GetUserEmailAsync(Guid userId);
        Task<string?> GetUserNameAsync(Guid userId);
    }
}
