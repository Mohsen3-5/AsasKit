using Microsoft.AspNetCore.Identity;

namespace Asas.Identity.Domain.Entities
{
    public class AsasUserClaim : IdentityUserClaim<Guid> { public int? TenantId { get; set; } }

}
