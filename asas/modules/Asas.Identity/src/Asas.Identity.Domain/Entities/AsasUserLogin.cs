using Microsoft.AspNetCore.Identity;

namespace Asas.Identity.Domain.Entities
{
    public class AsasUserLogin : IdentityUserLogin<Guid> { public int? TenantId { get; set; } }

}
