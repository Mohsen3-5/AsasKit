using Asas.Core.EF;

namespace Asas.Identity.Domain.Entities
{
    public class UserDevice 
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public AsasUser User { get; set; }

        public string DeviceToken { get; set; } = default!;
        public string DeviceType { get; set; } = default!;
        public string? DeviceModel { get; set; }
        public string? PlatformVersion { get; set; }
        public bool IsActive { get; set; } = true;

        public DateTime? CreatedAtUtc { get; set; }

    }
}
