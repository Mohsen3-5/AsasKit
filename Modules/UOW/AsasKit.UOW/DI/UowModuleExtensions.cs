using AsasKit.UOW.Behaviors;
using AsasKit.UOW.Options;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AsasKit.UOW.DI;

public static class UowModuleExtensions
{
    /// <summary>
    /// Registers UoW options + MediatR UnitOfWorkBehavior. 
    /// NOTE: You must map IUnitOfWork to your primary DbContext in composition root.
    /// </summary>
    public static IServiceCollection AddUowModule(
        this IServiceCollection services,
        IConfiguration? config = null,
        Action<UowOptions>? configure = null)
    {
        if (config is not null)
            services.Configure<UowOptions>(config.GetSection("Uow"));
        if (configure is not null)
            services.Configure(configure);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>));

        return services;
    }
}
