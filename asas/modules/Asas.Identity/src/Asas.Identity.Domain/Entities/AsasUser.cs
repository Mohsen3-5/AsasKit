using Microsoft.AspNetCore.Identity;

namespace Asas.Identity.Domain.Entities
{
    public class AsasUser : IdentityUser<Guid>   // <- was sealed; now open for inheritance
    {
        public Guid TenantId { get; set; }
        public string? ProfileJson { get; set; }  // optional: for extra typed data
    }

}
