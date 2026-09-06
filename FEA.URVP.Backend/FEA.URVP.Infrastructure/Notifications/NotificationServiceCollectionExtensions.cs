using FEA.URVP.Application.Abstractions.Notifications;
using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Options;
using FEA.URVP.Application.Services.Notifications;
using FEA.URVP.Infrastructure.Email;
using FEA.URVP.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FEA.URVP.Infrastructure.Notifications;

public static class NotificationServiceCollectionExtensions
{
    public static IServiceCollection AddNotificationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<NotificationSettingsOptions>(
            configuration.GetSection(NotificationSettingsOptions.SectionName));
        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));
        services.Configure<NotificationCacheOptions>(
            configuration.GetSection(NotificationCacheOptions.SectionName));

        services.AddDistributedMemoryCache();

        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationOutboxRepository, NotificationOutboxRepository>();
        services.AddScoped<IEmailLogRepository, EmailLogRepository>();
        services.AddScoped<NotificationBusinessRulesService>();
        services.AddScoped<NotificationValidationService>();
        services.AddScoped<INotificationApplicationService, NotificationApplicationService>();
        services.AddScoped<IUserEmailService, UserEmailService>();
        services.AddScoped<IEmailService, SmtpEmailService>();
        services.AddScoped<INotificationOutboxService, NotificationOutboxService>();
        services.AddScoped<IPushNotificationService, LoggingPushNotificationService>();
        services.AddSingleton<INotificationCacheService, NotificationCacheService>();

        // Hangfire is not in this repository. Do not add a Hangfire job alongside this processor.
        services.AddHostedService<NotificationOutboxProcessor>();

        return services;
    }
}
