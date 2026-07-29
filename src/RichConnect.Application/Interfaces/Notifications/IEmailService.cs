namespace RICHConnect.Backend.Application.Interfaces.Notifications
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string toEmail, string toName, string subject, string body, string? actionUrl = null, string? actionText = null);
        Task<bool> SendEmailAsync(string toEmail, string toName, string templateType, Dictionary<string, string> templateData);
        Task<bool> SendBulkEmailAsync(List<(string email, string name)> recipients, string subject, string body);
        Task<bool> SendEmailFromUserAsync(string fromEmail, string fromName, string toEmail, string toName, string subject, string body);
        bool IsEmailConfigured();
        string GetEmailConfigurationStatus();
    }
}
