using Asas.Core.EF;
using Asas.Tenancy.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Configuration;

namespace Asas.Tenancy.Infrastructure;

public abstract class BaseAsasDbContext<TDbContext> : DbContext
    where TDbContext : DbContext
{
    private readonly ICurrentTenant _tenant;
    private readonly IConfiguration _configuration;
    protected BaseAsasDbContext(DbContextOptions<TDbContext> options, ICurrentTenant? tenant, IConfiguration configuration)
        : base(options)
    {
        _tenant = tenant;
        _configuration = configuration;
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        var enabledRaw = _configuration["Features:EnableTenancy"];
        var enableTenancy = bool.TryParse(enabledRaw, out var b) && b;

        // Apply global tenant filter to all entities implementing IMultiTenant
        if (enableTenancy)
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
            if (entry.State == EntityState.Added && entry.Entity.TenantId == null)
            {
                entry.Entity.TenantId = _tenant.Id;
            }
        }
    }
}
