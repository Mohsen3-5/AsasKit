using Asas.Identity.Domain.Entities;
using Asas.Tenancy.Contracts;
using Asas.Tenancy.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CleanTestApp.Infrastructure
{
    public class AppDbContext : BaseAsasDbContext<AppDbContext>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentTenant? tenant)
            : base(options, tenant)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ---------- Reuse Identity tables in App context (no migrations here) ----------
            modelBuilder.Ignore<UserDevice>();

            modelBuilder.Entity<AsasUser>(e =>
            {
                e.ToTable("AsasUsers", tb => tb.ExcludeFromMigrations());
                e.HasKey(u => u.Id);
            });

            modelBuilder.Entity<AsasRole>(e =>
            {
                e.ToTable("AsasRoles", tb => tb.ExcludeFromMigrations());
                e.HasKey(r => r.Id);
            });

            modelBuilder.Entity<AsasUserClaim>(e =>
            {
                e.ToTable("AsasUserClaims", tb => tb.ExcludeFromMigrations());
                e.HasKey(c => c.Id);
            });

            modelBuilder.Entity<AsasRoleClaim>(e =>
            {
                e.ToTable("AsasRoleClaims", tb => tb.ExcludeFromMigrations());
                e.HasKey(rc => rc.Id);
            });

            modelBuilder.Entity<AsasUserLogin>(e =>
            {
                e.ToTable("AsasUserLogins", tb => tb.ExcludeFromMigrations());
                e.HasKey(l => new { l.LoginProvider, l.ProviderKey });
            });

            modelBuilder.Entity<AsasUserRole>(e =>
            {
                e.ToTable("AsasUserRoles", tb => tb.ExcludeFromMigrations());
                e.HasKey(ur => new { ur.UserId, ur.RoleId });
            });

            modelBuilder.Entity<AsasUserToken>(e =>
            {
                e.ToTable("AsasUserTokens", tb => tb.ExcludeFromMigrations());
                e.HasKey(t => new { t.UserId, t.LoginProvider, t.Name });
            });

            // Configure your entities here
        }
    }
}
