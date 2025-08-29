using Asas.Core.Modularity;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public abstract class AsasModule : IAsasModule
{
    public virtual void PreConfigureServices(IServiceCollection services) { }
    public virtual void ConfigureServices(IServiceCollection services, IConfiguration cfg) { }
    public virtual void OnApplicationInitialization(IApplicationBuilder app) { }
}
