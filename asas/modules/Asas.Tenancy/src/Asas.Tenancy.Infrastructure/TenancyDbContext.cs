using System.Reflection.Emit;
using Asas.Tenancy.Contracts;
using Asas.Tenancy.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Asas.Tenancy.Infrastructure;

public sealed class TenancyDbContext : BaseAsasDbContext<TenancyDbContext>
{

    public TenancyDbContext(DbContextOptions<TenancyDbContext> options, ICurrentTenant tenant) : base(options, tenant)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<Tenant>().HasData(new Tenant
        {
            Id = 1,
            Name = "Default Tenant",
            Identifier = "default",
            Host = "localhost",
            IsActive = true
        });

        // Pull in any IEntityTypeConfiguration<T> you define in this assembly
        b.ApplyConfigurationsFromAssembly(typeof(TenancyDbContext).Assembly);
    }
}

