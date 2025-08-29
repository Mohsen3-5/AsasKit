using Microsoft.AspNetCore.Identity;

namespace Asas.Identity.Domain.Entities
{
    public class AsasRoleClaim : IdentityRoleClaim<Guid> { public Guid TenantId { get; set; } }
}
