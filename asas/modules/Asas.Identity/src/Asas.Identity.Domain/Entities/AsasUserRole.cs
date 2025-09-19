using Microsoft.AspNetCore.Identity;

namespace Asas.Identity.Domain.Entities
{
    public class AsasUserRole : IdentityUserRole<Guid>
    {
        public int? TenantId { get; set; }
    }

}
