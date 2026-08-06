using FEA.URVP.Application.Abstractions.Events;
using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Infrastructure.Data.Context;
using FEA.URVP.Infrastructure.Events;
using FEA.URVP.Infrastructure.Repositories;
using Microsoft.AspNetCore.DataProtection;
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
        var connectionString = configuration.GetConnectionString("SqlServerConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'SqlServerConnection' is missing. " +
                "Set ConnectionStrings:SqlServerConnection in configuration.");

        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IStudentProfileRepository, StudentProfileRepository>();
        services.AddScoped<IProjectRankingRepository, ProjectRankingRepository>();
        services.AddScoped<IFileStorageRepository, FileStorageRepository>();
        services.AddScoped<IEventBus, InMemoryEventBus>();

        // Persist data-protection keys so OIDC correlation/state survives restarts.
        services.AddDataProtection()
            .SetApplicationName("FEA.URVP.Backend")
            .PersistKeysToDbContext<AppDbContext>();

        return services;
    }
}
