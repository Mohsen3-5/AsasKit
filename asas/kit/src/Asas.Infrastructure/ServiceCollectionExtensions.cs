using System.Reflection;
using Asas.Core.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Asas.Infrastructure.Repositories
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers IRepository<TEntity> -> EfRepository<TEntity, TDbContext> for all concrete Entity types
        /// found in the provided assemblies.
        /// </summary>
        public static IServiceCollection AddEfRepositoriesFor<TDbContext>(
            this IServiceCollection services,
            params Assembly[] assemblies)
            where TDbContext : DbContext
        {
            var entityBase = typeof(Entity);
            var repoInterfaceOpen = typeof(IRepository<>);
            var repoImplOpen = typeof(EfRepository<,>);
            var dbCtxType = typeof(TDbContext);

            foreach (var asm in assemblies.Distinct())
            {
                var entities = asm
                    .GetTypes()
                    .Where(t =>
                        !t.IsAbstract &&
                        !t.IsGenericTypeDefinition &&
                        entityBase.IsAssignableFrom(t));

                foreach (var entityType in entities)
                {
                    var serviceType = repoInterfaceOpen.MakeGenericType(entityType);
                    var implType = repoImplOpen.MakeGenericType(entityType, dbCtxType);

                    // Avoid duplicate registrations if called multiple times
                    services.TryAddScoped(serviceType, implType);
                }
            }

            return services;
        }

        /// <summary>
        /// Convenience overload: scan the assembly of TMarker for Entity types.
        /// </summary>
        public static IServiceCollection AddEfRepositoriesFor<TDbContext, TMarker>(
            this IServiceCollection services)
            where TDbContext : DbContext
        {
            return services.AddEfRepositoriesFor<TDbContext>(typeof(TMarker).Assembly);
        }
    }
}
