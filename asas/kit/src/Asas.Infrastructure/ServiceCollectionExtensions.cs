using Asas.Core.Abstractions;
using Asas.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Asas.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEfRepository<TEntity, TKey, TDbContext>(this IServiceCollection services)
        where TEntity : class
        where TDbContext : DbContext
    {
        services.AddScoped<IRepository<TEntity, TKey>, EfRepository<TEntity, TKey, TDbContext>>();
        return services;
    }
}
