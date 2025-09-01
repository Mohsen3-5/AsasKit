using System.Reflection;
using Asas.Messaging.Abstractions;
using Asas.Messaging.Publishing;
using Microsoft.Extensions.DependencyInjection;

namespace Asas.Messaging.DI;
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
