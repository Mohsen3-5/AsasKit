using System.Text;
using Asas.Tenancy.Infrastructure.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;

namespace Asas.Tenancy.Infrastructure
{
    public static class TenancyDbContextRegistrationExtensions
    {
        // Registers TContext with provider/connection from host config, then enables row-level multitenancy
        public static IServiceCollection AddTenantedDbContext<TContext>(
           this IServiceCollection services,
           IConfiguration cfg,
           string? connectionString = null,
           string provider = "sqlserver")
           where TContext : DbContext
        {
            services.AddScoped<TenantSaveChangesInterceptor>();

            // ----- Connection string -----
            var cs = connectionString ?? cfg.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(cs))
            {
                if (string.Equals(provider, "sqlite", StringComparison.OrdinalIgnoreCase))
                {
                    cs = "Data Source=tenancy.db";
                }
                else
                {
                    throw new InvalidOperationException(
                        "No connection string provided. Set ConnectionStrings:Default or pass connectionString.");
                }
            }

            // ----- DbContext + migrations assembly -----
            services.AddDbContext<TContext>((sp, db) =>
            {
                var migAsm = typeof(TContext).Assembly.FullName;
                switch ((provider ?? "sqlserver").Trim().ToLowerInvariant())
                {
                    case "postgres":
                    case "postgresql":
                        db.UseNpgsql(cs, x => x.MigrationsAssembly(migAsm));
                        break;
                    case "sqlite":
                        db.UseSqlite(cs, x => x.MigrationsAssembly(migAsm));
                        break;
                    default: // sqlserver
                        db.UseSqlServer(cs, x => x.MigrationsAssembly(migAsm));
                        break;
                }
                // enable shadow TenantId + global filter + insert stamping
                db.ReplaceService<IModelCustomizer, TenancyModelCustomizer>();
                db.AddInterceptors(sp.GetRequiredService<TenantSaveChangesInterceptor>());
            });



            return services;
        }

    }
}
