// Asas.Core.Modularity/ServiceCollectionExtensions.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Asas.Core.Modularity;

public static class ServiceCollectionExtensions
{
    // ABP-like: AddApplication<TStartupModule>()
    public static IServiceCollection AddApplication<TStartupModule>(
        this IServiceCollection services,
        IConfiguration cfg)
        where TStartupModule : AsasModule
    {
        var orderedTypes = ModuleDiscovery.ResolveDependencyGraph(typeof(TStartupModule));
        var instances = orderedTypes.Select(t => (AsasModule)Activator.CreateInstance(t)!).ToList();

        // Run ConfigureServices in dependency order
        foreach (var m in instances)
            m.ConfigureServices(services, cfg);

        // Store catalog for runtime pipeline hooks
        services.AddSingleton(new AsasModuleCatalog(instances));
        return services;
    }

    // ABP-like: InitializeApplication()
    public static IApplicationBuilder InitializeApplication(this IApplicationBuilder app)
    {
        var catalog = app.ApplicationServices.GetRequiredService<AsasModuleCatalog>();
        foreach (var m in catalog.ModulesInOrder)
            m.OnApplicationInitialization(app);
        return app;
    }
}
