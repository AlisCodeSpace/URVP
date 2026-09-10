using FEA.URVP.Application.Abstractions.Directory;
using FEA.URVP.Application.Abstractions.Events;
using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Abstractions.Security;
using FEA.URVP.Infrastructure.Data.Context;
using FEA.URVP.Infrastructure.DataProtection;
using FEA.URVP.Infrastructure.Ldap;
using FEA.URVP.Infrastructure.Events;
using FEA.URVP.Infrastructure.Notifications;
using FEA.URVP.Infrastructure.Repositories;
using FEA.URVP.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FEA.URVP.Infrastructure;

/// <summary>
/// Infrastructure-layer service registration (persistence, event bus).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = SqlConnectionString.Normalize(
            configuration.GetConnectionString("SqlServerConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'SqlServerConnection' is missing. " +
                "Set ConnectionStrings:SqlServerConnection in configuration."),
            configuration);

        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IStudentProfileRepository, StudentProfileRepository>();
        services.AddScoped<IProjectRankingRepository, ProjectRankingRepository>();
        services.AddScoped<IFacultyCandidateRankingRepository, FacultyCandidateRankingRepository>();
        services.AddScoped<IMatchingRunRepository, MatchingRunRepository>();
        services.AddScoped<IFileStorageRepository, FileStorageRepository>();
        services.AddScoped<IValueListRepository, ValueListRepository>();
        services.AddScoped<IDivisionRepository, DivisionRepository>();
        services.AddScoped<INewsArticleRepository, NewsArticleRepository>();
        services.AddScoped<IWorkshopRepository, WorkshopRepository>();
        services.AddScoped<ISemesterRepository, SemesterRepository>();
        services.AddScoped<IAdminOverviewReadRepository, AdminOverviewReadRepository>();
        services.AddScoped<IEmailSettingsRepository, EmailSettingsRepository>();
        services.AddScoped<IHomeIntroRepository, HomeIntroRepository>();
        services.AddSingleton<ISmtpCredentialProtector, DataProtectionSmtpCredentialProtector>();
        services.AddNotificationServices(configuration);
        services.AddScoped<IEventBus, InMemoryEventBus>();
        services.AddScoped<IDirectoryGroupLookup, LdapDirectoryGroupLookup>();
        services.AddUrvpDataProtection(configuration);

        return services;
    }
}
