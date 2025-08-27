using AsasKit.Modules.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AsasKit.Modules.Identity;

// Generic base so hosts can plug a derived user
public class AsasIdentityDbContext<TUser> : IdentityDbContext<TUser, IdentityRole<Guid>, Guid>
    where TUser : AsasUser
{
    public AsasIdentityDbContext(DbContextOptions options) : base(options) { }
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<RefreshToken>(e =>
        {
            e.ToTable("RefreshTokens");
            e.HasKey(x => x.Id);
            e.Property(x => x.TokenHash).IsRequired().HasMaxLength(128);
            e.Property(x => x.Device).HasMaxLength(128);
            e.HasIndex(x => new { x.TenantId, x.UserId });
            e.HasIndex(x => x.TokenHash).IsUnique();
        });
    }
}
