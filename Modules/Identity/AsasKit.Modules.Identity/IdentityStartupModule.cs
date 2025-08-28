using AsasKit.Modules.Identity.Contracts;
using AsasKit.Modules.Identity.Repo;
using Kernel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AsasKit.Modules.Identity;

public sealed class IdentityStartupModule : AsasModule
{
    public override void ConfigureServices(IServiceCollection services, IConfiguration cfg)
    {
        var provider = cfg["Data:Provider"] ?? "sqlserver";
        var cs = cfg.GetConnectionString("Default");

        // your extension we wrote earlier (non-generic)
        services.AddIdentityModule(cfg, cs, provider);
        services.AddScoped<IUserDirectory, UserDirectory>();

    }

    public override void OnApplicationInitialization(IApplicationBuilder app)
    {
        // middleware must run before mapping endpoints
        app.UseAuthentication();
        app.UseAuthorization();

        // map /auth endpoints
        if (app is IEndpointRouteBuilder ep)
            ep.MapIdentityEndpoints();
    }


}
