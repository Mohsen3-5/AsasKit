using Microsoft.AspNetCore.Identity;

namespace Asas.Identity.Domain.Entities
{
    public class AsasUser : IdentityUser<Guid>   // <- was sealed; now open for inheritance
    {
        public string? DeviceToken { get; set; } = null;
        public int? TenantId { get; set; }
        public string? ProfileJson { get; set; }  // optional: for extra typed data
    }

}
