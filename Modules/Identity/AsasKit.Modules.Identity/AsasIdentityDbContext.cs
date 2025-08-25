using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AsasKit.Modules.Identity;

// Generic base so hosts can plug a derived user
public class AsasIdentityDbContext<TUser> : IdentityDbContext<TUser, IdentityRole<Guid>, Guid>
    where TUser : AsasUser
{
    public AsasIdentityDbContext(DbContextOptions options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<TUser>(e =>
        {
            e.HasIndex(x => new { x.Email, x.TenantId }).HasDatabaseName("IX_User_Email_Tenant");
        });
    }
}
