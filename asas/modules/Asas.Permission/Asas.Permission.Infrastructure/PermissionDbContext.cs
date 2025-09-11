using Asas.Permission.Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace Asas.Permission.Infrastructure
{
    public class PermissionDbContext : DbContext
    {
        public PermissionDbContext(DbContextOptions<PermissionDbContext> options) : base(options) { }

        public DbSet<AsasPermission> Permissions => Set<AsasPermission>();
        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
        public DbSet<UserPermissionOverride> UserPermissionOverrides => Set<UserPermissionOverride>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            b.Entity<AsasPermission>(e =>
            {
                e.ToTable("Permissions");
                e.HasKey(x => x.Id);
                e.HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
                e.Property(x => x.Name).HasMaxLength(256).IsRequired();
                e.Property(x => x.Group).HasMaxLength(128);
            });

            b.Entity<RolePermission>(e =>
            {
                e.ToTable("RolePermissions");
                e.HasKey(x => x.Id);
                e.HasIndex(x => new { x.TenantId, x.RoleId, x.PermissionName }).IsUnique();
                e.Property(x => x.PermissionName).HasMaxLength(256).IsRequired();
            });

            b.Entity<UserPermissionOverride>(e =>
            {
                e.ToTable("UserPermissionOverrides");
                e.HasKey(x => x.Id);
                e.HasIndex(x => new { x.TenantId, x.UserId, x.PermissionName }).IsUnique();
                e.Property(x => x.PermissionName).HasMaxLength(256).IsRequired();
            });
        }
    }

}
