using RICHConnect.Backend.Application.BackgroundServices;
using RICHConnect.Backend.Application.Interfaces.Notifications;
using RICHConnect.Backend.Application.Services.Notifications;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Notifications;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Notifications.Interfaces;

namespace RICHConnect.Backend.Api.Configuration.DependencyInjection
{
    /// <summary>
    /// Configuration for notification-related services
    /// </summary>
    public static class NotificationServicesConfiguration
    {
        /// <summary>
        /// Register all notification-related services
        /// </summary>
        public static IServiceCollection AddNotificationServices(this IServiceCollection services)
        {
            // Repositories
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<INotificationOutboxRepository, NotificationOutboxRepository>();
            
            // Application Services
            services.AddScoped<INotificationApplicationService, NotificationApplicationService>();
            services.AddScoped<NotificationBusinessRulesService>();
            services.AddScoped<NotificationValidationService>();
            
            // Email Services
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IUserEmailService, UserEmailService>();
            
            // Outbox Services
            services.AddScoped<NotificationOutboxService>();
            // Use Hangfire-based background processing instead of a custom BackgroundService
            services.AddScoped<NotificationOutboxHangfireJob>();
            
            // Push Notification Services
            services.AddScoped<IPushNotificationService, PushNotificationService>();
            
            // Notification Cache Services
            services.AddSingleton<INotificationCacheService, NotificationCacheService>();
            
            return services;
        }
    }
}
