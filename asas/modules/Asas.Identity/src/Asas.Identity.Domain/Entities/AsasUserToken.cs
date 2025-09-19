
using Microsoft.AspNetCore.Identity;

namespace Asas.Identity.Domain.Entities
{
    public class AsasUserToken : IdentityUserToken<Guid> { public int? TenantId { get; set; } }

}
