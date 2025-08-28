using System.Reflection;
using AsasKit.Core.Abstractions;
using AsasKit.Shared.Messaging.Publishing;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAsasKitMessaging(
        this IServiceCollection services,
        params Assembly[] handlerAssemblies)
    {
        var handlerAsms = handlerAssemblies is { Length: > 0 } ? handlerAssemblies : Array.Empty<Assembly>();

        // Let MediatR discover the open-generic adapters
        var mediatRAsms = handlerAsms
            .Distinct()
            .ToArray();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(mediatRAsms));

     
        services.AddScoped<IEventPublisher, MediatREventPublisher>();
        return services;
    }
}
