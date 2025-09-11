using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace Asas.Permission.Api;

public class AsasPermissionApiModule : AsasModule
{
    public override void ConfigureServices(IServiceCollection services, IConfiguration cfg)
    {
        var logger = services.BuildServiceProvider().
           GetRequiredService<ILogger<AsasPermissionApiModule>>();
        var provider = cfg["Data:Provider"] ?? "sqlserver";
        var cs = cfg.GetConnectionString("Default");
        services.AddPermissionModule(cfg, cs, provider); 
    }

    public override void OnApplicationInitialization(IApplicationBuilder app)
    {
        var logger = app.ApplicationServices.GetRequiredService<ILogger<AsasPermissionApiModule>>();
        logger.LogInformation("Initializing Permission API Module...");


        logger.LogInformation("Permission API Module initialized successfully");
    }


}
