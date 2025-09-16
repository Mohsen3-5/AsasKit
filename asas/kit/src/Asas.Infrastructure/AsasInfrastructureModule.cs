
using Asas.Core.Abstractions;
using Asas.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Asas.Infrastructure
{
    public class AsasInfrastructureModule : AsasModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration cfg)
        {

            services.AddScoped(typeof(IRepository<,>), typeof(EfRepository<,,>));
        }
    }
}
