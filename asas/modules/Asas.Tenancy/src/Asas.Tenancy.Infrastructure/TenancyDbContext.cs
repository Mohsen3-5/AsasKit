using System.Reflection.Emit;
using Asas.Tenancy.Contracts;
using Asas.Tenancy.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Asas.Tenancy.Infrastructure; 

public sealed class TenancyDbContext : DbContext
{
    private readonly ICurrentTenant _tenant;
    private readonly ILogger<TenancyDbContext> _log;

    public Guid CurrentTenantId => _tenant.IsSet ? _tenant.Id : Guid.Empty;
    public bool TenantFilterEnabled { get; set; } = true;
    public TenancyDbContext(DbContextOptions<TenancyDbContext> options, ICurrentTenant tenant , ILogger<TenancyDbContext> log) : base(options) {
        _tenant = tenant; 
        _log = log;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        //b.ApplyTenantQueryFilters(this, _log, except: [typeof(Tenant)]); // never filter the Tenants table

        // Pull in any IEntityTypeConfiguration<T> you define in this assembly
        b.ApplyConfigurationsFromAssembly(typeof(TenancyDbContext).Assembly);
    }
}

