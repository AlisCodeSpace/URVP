using System.Reflection;
using FEA.URVP.Application.Abstractions.Events;
using FEA.URVP.Application.Abstractions.Files;
using FEA.URVP.Application.Behaviors;
using FEA.URVP.Application.Files;
using FEA.URVP.Application.Options;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FEA.URVP.Application;

/// <summary>
/// Application-layer service registration (MediatR, validation, event handlers).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        // Registers IEventHandler<T> implementations, including the four notification
        // lifecycle handlers (Created / Read / Deleted / SettingsUpdated).
        services.AddEventHandlers(assembly);
        services.Configure<FileStorageOptions>(configuration.GetSection(FileStorageOptions.SectionName));
        services.AddScoped<IMimeTypeValidator, MimeTypeValidator>();

        return services;
    }

    private static void AddEventHandlers(this IServiceCollection services, Assembly assembly)
    {
        var handlerInterface = typeof(IEventHandler<>);

        foreach (var type in assembly.GetTypes().Where(t => t is { IsAbstract: false, IsInterface: false }))
        {
            var service = type.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterface);

            if (service is not null)
            {
                services.AddScoped(service, type);
            }
        }
    }
}
