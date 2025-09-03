using Asas.Tenancy.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
namespace Asas.Tenancy.Api;

public class AsasTenancyApiModule : AsasModule
{
    public override void ConfigureServices(IServiceCollection services, IConfiguration cfg)
    {
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

        services.AddTenantedDbContext<AppDbContext>(cfg); // <-- replace with your actual DbContext type

    }

    public override void OnApplicationInitialization(IApplicationBuilder app)
    {
        app.UseTenancy(); // after UseAuthentication() if you resolve from claims
        app.ApplicationServices.GetRequiredService<ILogger<AsasTenancyApiModule>>()
           .LogInformation("Tenancy initialized.");
    }
}
