using AsasKit.Domain.Tenancy;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AsasKit.Infrastructure.Models;
using Microsoft.AspNetCore.Identity;

namespace AsasKit.Infrastructure.Data;

public sealed class AppDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
{
    private readonly Guid _tenantId;
    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantAccessor accessor) : base(options)
    {
        _tenantId = accessor.CurrentTenantId;
    }

    public DbSet<Membership> Memberships => Set<Membership>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
    }
}

// Local interface to avoid circular dep into Application
public interface ITenantAccessor { Guid CurrentTenantId { get; } }