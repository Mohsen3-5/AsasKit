using Asas.Tenancy.Contracts;
using Asas.Tenancy.Infrastructure.Ef;
using Asas.Tenancy.Infrastructure.Runtime;
using Asas.Tenancy.Infrastructure.Runtime.Resolvers;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Asas.Tenancy.Api;

public static class TenancyModulesExtensions
{
    // Pure code-based registration; host passes delegates. No IConfiguration needed.
    public static IServiceCollection AddTenancyModule(
        this IServiceCollection services,
        Action<TenancyOptions>? runtime = null,
        Action<TenancyModelOptions>? model = null)
    {
        services.AddHttpContextAccessor();


        // ... rest of the file remains unchanged
        // runtime options + services
        services.Configure(runtime ?? (_ => { }));
        services.AddSingleton<ITenantAccessor, DefaultTenantAccessor>();
        services.AddTransient<RouteTenantResolver>();
        services.AddTransient<HeaderTenantResolver>();
        services.AddTransient<ClaimsTenantResolver>();
        services.AddTransient<SubdomainTenantResolver>();
        services.AddTransient<ITenantResolver, CompositeTenantResolver>();

        // EF conventions
        services.Configure(model ?? (o =>
        {
            o.ScopeAllByDefault = true;
            o.IncludeNamespaces.Add("Asas.");        // scope your app by default
            o.ExcludeNamespaces.Add("Asas.Tenancy"); // not the tenancy module, migrations, etc.
        }));
        services.AddScoped<TenantSaveChangesInterceptor>();

        return services;
    }

    public static IApplicationBuilder UseTenancy(this IApplicationBuilder app)
        => app.UseMiddleware<TenantResolutionMiddleware>();

    // Call inside AddDbContext((sp,o) => o.EnableRowLevelMultitenancy(sp))
    public static DbContextOptionsBuilder EnableRowLevelMultitenancy(this DbContextOptionsBuilder o, IServiceProvider sp)
    {
        o.ReplaceService<IModelCustomizer, TenancyModelCustomizer>();
        o.AddInterceptors(sp.GetRequiredService<TenantSaveChangesInterceptor>());
        return o;
    }
}
