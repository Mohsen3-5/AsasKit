﻿using Asas.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Asas.Identity.Infrastructure
{
    // Generic base so hosts can plug a derived user if needed
    public class AsasIdentityDbContext : AsasIdentityDbContextBase<AsasUser>
    {
        public AsasIdentityDbContext(DbContextOptions<AsasIdentityDbContext> options) : base(options) { }

        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            // Map identity tables (this context OWNS them)
            b.Entity<AsasUser>(e => e.ToTable("AsasUsers"));
            b.Entity<AsasRole>(e => e.ToTable("AsasRoles"));
            b.Entity<AsasUserClaim>(e => e.ToTable("AsasUserClaims"));
            b.Entity<AsasRoleClaim>(e => e.ToTable("AsasRoleClaims"));
            b.Entity<AsasUserLogin>(e => e.ToTable("AsasUserLogins"));
            b.Entity<AsasUserRole>(e => e.ToTable("AsasUserRoles"));
            b.Entity<AsasUserToken>(e => e.ToTable("AsasUserTokens"));

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
}
