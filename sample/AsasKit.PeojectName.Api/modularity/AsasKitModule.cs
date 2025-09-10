using Asas.Core.Modularity;
using Asas.Identity.Api;
using Asas.Identity.Infrastructure;
using Asas.Messaging.DI;
using Asas.Tenancy.Api;
using Asas.Tenancy.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AsasKit.ProjectName.Api.modularity
{
    [DependsOn(
    typeof(AsasIdentityApiModule),
    typeof(AsasTenancyApiModule)
    )]
    public class AsasKitModule : AsasModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration cfg)
        {
            // host-level stuff (Swagger, CORS, etc.)
            services.AddControllers();
            services.AddHttpContextAccessor();
            services.AddOpenApi();

            services.AddAsasKitMessaging(
                typeof(IdentityAppAssemblyMarker).Assembly,
                typeof(Program).Assembly
            );

        }

        public override void OnApplicationInitialization(IApplicationBuilder app)
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<AsasIdentityApiModule>();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
        }
    }
}