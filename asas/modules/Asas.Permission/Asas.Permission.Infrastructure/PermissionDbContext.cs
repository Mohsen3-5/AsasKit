using Asas.Permission.Domain.Entity;
using Asas.Tenancy.Contracts;
using Asas.Tenancy.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Asas.Permission.Infrastructure
{
    public class PermissionDbContext : BaseAsasDbContext<PermissionDbContext>
    {
        public PermissionDbContext(DbContextOptions<PermissionDbContext> options, ICurrentTenant tenant)
        : base(options, tenant)
        {
        }

        public DbSet<AsasPermission> AsasPermission => Set<AsasPermission>();
        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
        public DbSet<UserPermissionOverride> UserPermissionOverrides => Set<UserPermissionOverride>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            b.Entity<AsasPermission>(e =>
            {
                e.ToTable("AsasPermissions");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).ValueGeneratedOnAdd();
                e.HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
                e.Property(x => x.Name).HasMaxLength(256).IsRequired();
                e.Property(x => x.Group).HasMaxLength(128);
            });

            b.Entity<RolePermission>(e =>
            {
                e.ToTable("RolePermissions");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).ValueGeneratedOnAdd(); // Guid generated client-side by EF
                e.HasIndex(x => new { x.TenantId, x.RoleId, x.PermissionName }).IsUnique();
                e.Property(x => x.PermissionName).HasMaxLength(256).IsRequired();
            });

            b.Entity<UserPermissionOverride>(e =>
            {
                e.ToTable("UserPermissionOverrides");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).ValueGeneratedOnAdd(); // Guid generated client-side by EF
                e.HasIndex(x => new { x.TenantId, x.UserId, x.PermissionName }).IsUnique();
                e.Property(x => x.PermissionName).HasMaxLength(256).IsRequired();
            });
        }
    }

}
