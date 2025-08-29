using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Asas.Core.Modularity
{
    public interface IAsasModule
    {
        void PreConfigureServices(IServiceCollection services);
        void ConfigureServices(IServiceCollection services, IConfiguration cfg);
        void OnApplicationInitialization(IApplicationBuilder app);
    }
}
