using Asas.Core.EF;
using Asas.Tenancy.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Asas.Tenancy.Infrastructure;

public abstract class BaseAsasDbContext<TDbContext> : DbContext
    where TDbContext : DbContext
{
    private readonly ICurrentTenant _tenant;

    protected BaseAsasDbContext(DbContextOptions<TDbContext> options, ICurrentTenant tenant)
        : base(options)
    {
        _tenant = tenant;
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply global tenant filter to all entities implementing IMultiTenant
        modelBuilder.ApplyTenantFilters(_tenant);
    }

    public override int SaveChanges()
    {
        ApplyTenantId();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyTenantId();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyTenantId()
    {
        foreach (var entry in ChangeTracker.Entries<Entity>())
        {
            if (entry.State == EntityState.Added && entry.Entity.TenantId == 0 && _tenant.IsSet)
            {
                entry.Entity.TenantId = _tenant.Id;
            }
        }
    }
}
