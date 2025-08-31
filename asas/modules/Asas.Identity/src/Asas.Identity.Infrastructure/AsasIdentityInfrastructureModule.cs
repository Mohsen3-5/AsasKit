using Asas.Core.Modularity;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Asas.Identity.Infrastructure
{
    public class AsasIdentityInfrastructureModule : IAsasModule
    {
        public void PreConfigureServices(IServiceCollection services)
        {
            throw new NotImplementedException();
        }
        public void ConfigureServices(IServiceCollection services, IConfiguration cfg)
        {
            throw new NotImplementedException();
        }
        public void OnApplicationInitialization(IApplicationBuilder app)
        {
            throw new NotImplementedException();
        }
    }
}
