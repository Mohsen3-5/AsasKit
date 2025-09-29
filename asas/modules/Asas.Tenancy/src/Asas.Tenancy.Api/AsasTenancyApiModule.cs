using Asas.Tenancy.Application;
using Asas.Tenancy.Contracts;
using Asas.Tenancy.Infrastructure;
using Asas.Tenancy.Infrastructure.EF;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
namespace Asas.Tenancy.Api;

public class AsasTenancyApiModule : AsasModule
{
    public override void ConfigureServices(IServiceCollection services, IConfiguration cfg)
    {
        var enableTenancy = cfg.GetValue<bool>("Features:EnableTenancy");
        if (!enableTenancy)
        {
            services.AddScoped<ICurrentTenant, NullCurrentTenant>();

            return; // skip tenancy setup completely
        }
        var provider = cfg["Data:Provider"] ?? "sqlserver";
        var cs = cfg.GetConnectionString("Default");
        // Tenancy — code-only config (no AddRange on IList)
        services.AddTenancyModule(
            runtime: o =>
            {
                o.ResolutionOrder.Clear();
                foreach (var s in new[] { "route", "header", "claims", "subdomain" })
                    o.ResolutionOrder.Add(s);

                o.HeaderName = "X-Tenant";
                o.RouteParamName = "tenant";
                // o.RootDomain = "asas.local";
            },
            model: o =>
            {
                o.ScopeAllByDefault = true;
                o.IncludeNamespaces.Add("Asas.");
                o.IncludeNamespaces.Add("AsasKit.");
                o.ExcludeNamespaces.Add("Asas.Tenancy");
                o.ExcludeNamespaces.Add("Microsoft.AspNetCore.Identity");
            });

        services.AddTenantedDbContext<TenancyDbContext>(cfg, cs, provider); // <-- replace with your actual DbContext type
        services.AddOptions<TenancyModelOptions>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentTenant, HostCurrentTenant>();
        services.AddScoped<ITenantStore, EfTenantStore>();


    }

    public override void OnApplicationInitialization(IApplicationBuilder app)
    {
        var cfg = app.ApplicationServices.GetRequiredService<IConfiguration>();
        var enableTenancy = cfg.GetValue<bool>("Features:EnableTenancy");
        var logger = app.ApplicationServices.GetRequiredService<ILogger<AsasTenancyApiModule>>();
        logger.LogInformation("EnableTenancy = {Flag}", enableTenancy);

        if (!enableTenancy)
        {
            // tenancy disabled → skip pipeline setup
            app.ApplicationServices.GetRequiredService<ILogger<AsasTenancyApiModule>>()
                .LogInformation("Tenancy is disabled. Skipping middleware.");
            return;
        }

        app.UseTenancy(); // after UseAuthentication() if you resolve from claims
        app.UseMiddleware<TenantResolutionMiddleware>();
        app.ApplicationServices.GetRequiredService<ILogger<AsasTenancyApiModule>>()
           .LogInformation("Tenancy initialized.");
    }

}
