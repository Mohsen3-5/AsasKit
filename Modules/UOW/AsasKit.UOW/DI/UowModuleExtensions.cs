// UoW registration as an extension (no module needed)
using AsasKit.UOW.Abstractions;
using AsasKit.UOW.Behaviors;
using AsasKit.UOW.Options;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

public static class UowServiceCollectionExtensions
{
    public static IServiceCollection AddUowFor<TDbContext>(this IServiceCollection services, IConfiguration cfg)
        where TDbContext : DbContext
    {
        services.Configure<UowOptions>(cfg.GetSection("Uow"));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>));

        services.AddScoped<IUnitOfWork>(sp =>
        {
            var db = sp.GetRequiredService<TDbContext>();
            if (db is IUnitOfWork uow) return uow;

            var ev = sp.GetRequiredService<AsasKit.Shared.Messaging.Abstractions.IEventPublisher>();
            var opt = sp.GetRequiredService<IOptions<UowOptions>>();
            return new DbContextUowAdapter<TDbContext>(db, ev, opt);
        });

        return services;
    }
}
