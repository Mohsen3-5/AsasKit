// Generic base (keep wherever it already lives)
using Asas.Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class AsasIdentityDbContextBase<TUser> : IdentityDbContext<TUser, IdentityRole<Guid>, Guid>
    where TUser : AsasUser
{
    public AsasIdentityDbContextBase(DbContextOptions options) : base(options) { }
    // shared mappings…
}
