using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AsasKit.Modules.Identity.Entities;

public class AsasUser : IdentityUser<Guid>   // <- was sealed; now open for inheritance
{
    public Guid TenantId { get; set; }
    public string? ProfileJson { get; set; }  // optional: for extra typed data
}

public class AsasRole : IdentityRole<Guid>
{
    public Guid TenantId { get; set; }
}

public class AsasUserRole : IdentityUserRole<Guid>
{
    public Guid TenantId { get; set; }
}

public class AsasUserClaim : IdentityUserClaim<Guid> { public Guid TenantId { get; set; } }
public class AsasUserLogin : IdentityUserLogin<Guid> { public Guid TenantId { get; set; } }
public class AsasRoleClaim : IdentityRoleClaim<Guid> { public Guid TenantId { get; set; } }
public class AsasUserToken : IdentityUserToken<Guid> { public Guid TenantId { get; set; } }

// Keep a default quick-start context that uses AsasUser
public class IdentityDbContext : AsasIdentityDbContext<AsasUser>
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> opts) : base(opts) { }
}
