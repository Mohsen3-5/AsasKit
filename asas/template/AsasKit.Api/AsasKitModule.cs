using Asas.Core.Modularity;
using Asas.Identity.Api;
using Asas.Permission.Api;
using Asas.Tenancy.Api;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AsasKit.Api
{
    [DependsOn(
        typeof(AsasIdentityApiModule),
        typeof(AsasTenancyApiModule),
        typeof(AsasPermissionApiModule)
        )]
    public class AsasKitModule : AsasModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration cfg)
        {
            services.AddHttpContextAccessor();
        }

        public override void OnApplicationInitialization(IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
        }
    }
}
