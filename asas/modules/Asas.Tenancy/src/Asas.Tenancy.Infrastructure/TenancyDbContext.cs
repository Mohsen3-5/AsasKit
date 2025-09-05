using Asas.Tenancy.Domain;
using Microsoft.EntityFrameworkCore;

namespace Asas.Tenancy.Infrastructure; 

public sealed class TenancyDbContext : DbContext
{
    public TenancyDbContext(DbContextOptions<TenancyDbContext> options) : base(options) { }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // Pull in any IEntityTypeConfiguration<T> you define in this assembly
        b.ApplyConfigurationsFromAssembly(typeof(TenancyDbContext).Assembly);
    }
}

