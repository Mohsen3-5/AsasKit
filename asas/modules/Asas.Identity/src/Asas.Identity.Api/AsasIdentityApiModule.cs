using Asas.Identity.Domain.Contracts;
using Asas.Identity.Infrastructure.Repo;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Asas.Identity.Api;

public class AsasIdentityApiModule : AsasModule
{
    public override void ConfigureServices(IServiceCollection services, IConfiguration cfg)
    {
        var provider = cfg["Data:Provider"] ?? "sqlserver";
        var cs = cfg.GetConnectionString("Default");

        services.AddIdentityModule(cfg, cs, provider); // registers DbContext, Identity, JWT, migrations assembly, etc.
        services.AddScoped<IUserDirectory, UserDirectory>();
    }

    public override void OnApplicationInitialization(IApplicationBuilder app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
    }
}
