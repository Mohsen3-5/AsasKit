using AsasKit.UOW.Abstractions;
using AsasKit.UOW.Behaviors;
using AsasKit.UOW.Options;
using Kernel;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

public class UowStartupModule<TDbContext> : AsasModule
    where TDbContext : DbContext  // relaxed constraint
{
    public override void ConfigureServices(IServiceCollection services, IConfiguration cfg)
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
    }
}
