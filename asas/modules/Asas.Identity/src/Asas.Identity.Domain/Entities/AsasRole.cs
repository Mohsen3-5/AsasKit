using Microsoft.AspNetCore.Identity;

namespace Asas.Identity.Domain.Entities
{
    public class AsasRole : IdentityRole<Guid>
    {
        public Guid TenantId { get; set; }
    }
}
