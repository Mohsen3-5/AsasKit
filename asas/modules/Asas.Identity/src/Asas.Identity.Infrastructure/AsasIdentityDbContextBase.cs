// Generic base (keep wherever it already lives)
using Asas.Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class AsasIdentityDbContextBase<TUser>
    : IdentityDbContext<
        TUser,
        AsasRole,                  
        Guid,
        AsasUserClaim,              
        AsasUserRole,              
        AsasUserLogin,              
        AsasRoleClaim,              
        AsasUserToken               
    >
    where TUser : AsasUser
{
    public AsasIdentityDbContextBase(DbContextOptions options) : base(options) { }
}
