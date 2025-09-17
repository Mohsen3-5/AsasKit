using System.Linq.Expressions;
using System.Reflection;
using Asas.Core.EF;
using Asas.Tenancy.Contracts;
using Microsoft.EntityFrameworkCore;
using EFF = Microsoft.EntityFrameworkCore.EF;

namespace Asas.Tenancy.Infrastructure;

public static class ModelBuilderTenantExtensions
{
    public static void ApplyTenantFilters(this ModelBuilder modelBuilder, ICurrentTenant tenant)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(Entity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(ModelBuilderTenantExtensions)
                    .GetMethod(nameof(SetTenantFilter), BindingFlags.NonPublic | BindingFlags.Static)!
                    .MakeGenericMethod(entityType.ClrType);

                method.Invoke(null, new object[] { modelBuilder, tenant });
            }
        }
    }

    private static void SetTenantFilter<TEntity>(ModelBuilder modelBuilder, ICurrentTenant tenant)
        where TEntity : Entity
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e => e.TenantId == tenant.Id);
    }

}
