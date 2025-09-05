using Asas.Identity.Domain.Contracts;
using Asas.Identity.Infrastructure.Repo;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Asas.Identity.Api;

public class AsasIdentityApiModule : AsasModule
{
    public override void ConfigureServices(IServiceCollection services, IConfiguration cfg)
    {
        var logger = services.BuildServiceProvider().
           GetRequiredService<ILogger<AsasIdentityApiModule>>();
        var provider = cfg["Data:Provider"] ?? "sqlserver";
        var cs = cfg.GetConnectionString("Default");
        logger.LogInformation("Identity API Module initialized successfully");
        logger.LogInformation("Using {Provider} database provider.", provider);
        logger.LogInformation("Using connection string: {ConnectionString}", cs);
        services.AddIdentityModule(cfg, cs, provider); 
    }

    public override void OnApplicationInitialization(IApplicationBuilder app)
    {
        var logger = app.ApplicationServices.GetRequiredService<ILogger<AsasIdentityApiModule>>();
        logger.LogInformation("Initializing Identity API Module...");

        app.UseAuthentication();
        app.UseAuthorization();
        IdentityModuleExtensions.MapIdentityEndpoints((Microsoft.AspNetCore.Routing.IEndpointRouteBuilder)app);

        logger.LogInformation("Identity API Module initialized successfully");
    }
}
