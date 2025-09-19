using Microsoft.AspNetCore.Identity;

namespace Asas.Identity.Domain.Entities
{
    public class AsasRole : IdentityRole<Guid>
    {
        public int? TenantId { get; set; }
    }
}
