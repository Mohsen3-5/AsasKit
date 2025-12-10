
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using Asas.Permission.Infrastructure;
using Asas.Permission.Contracts;
using Asas.Permission.Application;
namespace Asas.Permission.Api;

public static class PermissionModuleExtensions
{
    /// <summary>
    /// Quick-start registration using defaults:
    ///   TUser = AsasUser, DbContext = AsasIdentityDbContext&lt;AsasUser&gt;.
    /// Wires JWT, Identity, CurrentUser plumbing, TokenService, and (optionally)
    /// a non-generic IAuthService implementation if you have one.
    /// </summary>
    public static IServiceCollection AddPermissionModule(
        this IServiceCollection services,
        IConfiguration cfg,
        string? connectionString = null,
        string provider = "sqlserver")
    {
        services.AddPermissionModule<PermissionDbContext>(cfg, connectionString, provider);



        return services;
    }

    /// <summary>
    /// Advanced registration where host can swap the user type and DbContext.
    /// Requires AuthService&lt;TUser&gt; implementing IAuthService.
    /// </summary>
    public static IServiceCollection AddPermissionModule<TContext>(
        this IServiceCollection services,
        IConfiguration cfg,
        string? connectionString = null,
        string provider = "sqlserver")
        where TContext : DbContext
    {
        using var sp = services.BuildServiceProvider();

        // ----- Connection string -----
        var cs = connectionString ?? cfg.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(cs))
        {
            if (string.Equals(provider, "sqlite", StringComparison.OrdinalIgnoreCase))
            {
                cs = "Data Source=identity.db";
            }
            else
            {
                throw new InvalidOperationException(
                    "No connection string provided. Set ConnectionStrings:Default or pass connectionString.");
            }
        }

        // ----- DbContext + migrations assembly -----
        services.AddDbContext<TContext>(b =>
        {
            var migAsm = typeof(TContext).Assembly.FullName;
            switch ((provider ?? "sqlserver").Trim().ToLowerInvariant())
            {
                case "postgres":
                case "postgresql":
                    b.UseNpgsql(cs, x => x.MigrationsAssembly(migAsm));
                    break;
                case "sqlite":
                    b.UseSqlite(cs, x => x.MigrationsAssembly(migAsm));
                    break;
                default: // sqlserver
                    b.UseSqlServer(cs, x => x.MigrationsAssembly(migAsm));
                    break;
            }
        });
        services.AddSingleton<IPermissionDefinitionStore, PermissionDefinitionStore>();
        services.AddSingleton<IPermissionDefinitionProvider, SystemPermissionDefinitionProvider>();
        services.AddHostedService<PermissionSynchronizer>();

        services.AddScoped<IPermissionChecker, PermissionChecker>();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionHandler>();
        services.AddDistributedMemoryCache();
        return services;
    }
}
