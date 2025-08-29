// Asas.Identity.Api
using Asas.Core.Modularity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Asas.Identity.Api;

public class AsasIdentityApiModule : AsasModule
{
    public override void ConfigureServices(IServiceCollection services, IConfiguration cfg)
    {
        var provider = cfg["Data:Provider"] ?? "sqlserver";
        var cs = cfg.GetConnectionString("Default");

        //services.AddIdentityModule(cfg, cs, provider);
        // services.AddScoped<IUserDirectory, UserDirectory>();
    }
}
