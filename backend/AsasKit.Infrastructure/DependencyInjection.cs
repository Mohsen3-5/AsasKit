using AsasKit.Application.Abstractions.Persistence;
using AsasKit.Infrastructure.Data;
using AsasKit.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AsasKit.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration cfg)
    {
        services.AddOptions<DataOptions>()
                .Configure<IConfiguration>((opts, configuration) =>
                    configuration.GetSection("Data").Bind(opts));

        services.AddDbContext<AppDbContext>((sp, b) =>
        {
            var opts = sp.GetRequiredService<IOptions<DataOptions>>().Value;
            var provider = (opts.Provider ?? "Postgres").Trim().ToLowerInvariant();

            // Choose connection string priority: Data:ConnectionString → ConnectionStrings:Default → fallback
            var cs = string.IsNullOrWhiteSpace(opts.ConnectionString)
                ? cfg.GetConnectionString("Default")
                : opts.ConnectionString;

            // Quickstart fallback: if nothing configured, use a local SQLite file
            if (string.IsNullOrWhiteSpace(cs) && provider == "sqlite")
                cs = $"Data Source={System.IO.Path.Combine(AppContext.BaseDirectory, "asaskit.db")}";

            switch (provider)
            {
                case "postgres":
                    b.UseNpgsql(cs ?? "Host=localhost;Port=5432;Database=asaskit;Username=asaskit;Password=asaskit",
                        x => x.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
                    break;

                case "sqlserver":
                    b.UseSqlServer(cs ?? "Server=localhost;Database=AsasKit;Trusted_Connection=False;TrustServerCertificate=True;",
                        x => x.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
                    break;

                case "sqlite":
                    b.UseSqlite(cs ?? "Data Source=asaskit.db",
                        x => x.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
                    break;

                default:
                    throw new InvalidOperationException($"Unknown Data:Provider '{opts.Provider}'. Use Postgres | SqlServer | Sqlite.");
            }
        });


        // Repositories + UoW
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IMembershipRepository, MembershipRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
