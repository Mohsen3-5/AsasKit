using Asas.Core.Modularity;
using Asas.Identity.Api;
using Asas.Messaging.DI;
using AsasKit.Infrastructure.Data;
using AsasKit.Modules.Identity;

namespace AsasKit.Api
{
    [DependsOn(
    typeof(AsasIdentityApiModule)
    )]
    public class AsasKitModule : AsasModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration cfg)
        {
            // host-level stuff (Swagger, CORS, etc.)
            services.AddControllers();

            services.AddHttpContextAccessor();
            services.AddScoped<ITenantAccessor, HeaderTenantAccessor>();

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
