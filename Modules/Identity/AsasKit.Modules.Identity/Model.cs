using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AsasKit.Modules.Identity;

public class AsasUser : IdentityUser<Guid>   // <- was sealed; now open for inheritance
{
    public Guid TenantId { get; set; }
    public string? ProfileJson { get; set; }  // optional: for extra typed data
}

// Keep a default quick-start context that uses AsasUser
public class IdentityDbContext : AsasIdentityDbContext<AsasUser>
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> opts) : base(opts) { }
}
