using Asas.Tenancy.Infrastructure.Ef;    // TenancyModelCustomizer, TenantSaveChangesInterceptor
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Asas.Tenancy.Infrastructure
{
    public static class TenancyDbContextRegistrationExtensions
    {
        // Registers TContext with provider/connection from host config, then enables row-level multitenancy
        public static IServiceCollection AddTenantedDbContext<TContext>(
            this IServiceCollection services,
            IConfiguration cfg,
            string providerKey = "Data:Provider",
            string connectionName = "Default")
            where TContext : DbContext
        {
            var provider = (cfg[providerKey] ?? "sqlserver").ToLowerInvariant();
            var cs = cfg.GetConnectionString(connectionName)
                     ?? throw new InvalidOperationException($"ConnectionStrings:{connectionName} missing.");

            services.AddDbContext<TContext>((sp, db) =>
            {
                switch (provider)
                {
                    case "sqlserver": db.UseSqlServer(cs); break;
                    case "postgres":
                    case "npgsql": db.UseNpgsql(cs); break;
                    case "sqlite": db.UseSqlite(cs); break;
                    default: throw new NotSupportedException($"Unknown provider '{provider}'.");
                }

                // enable shadow TenantId + global filter + insert stamping
                db.ReplaceService<IModelCustomizer, TenancyModelCustomizer>();
                db.AddInterceptors(sp.GetRequiredService<TenantSaveChangesInterceptor>());
            });

            return services;
        }
    }
}
