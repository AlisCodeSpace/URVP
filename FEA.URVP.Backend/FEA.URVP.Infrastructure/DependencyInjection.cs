using FEA.URVP.Application.Abstractions.Events;
using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Infrastructure.Data.Context;
using FEA.URVP.Infrastructure.Events;
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
        services.AddScoped<IEventBus, InMemoryEventBus>();

        return services;
    }
}
