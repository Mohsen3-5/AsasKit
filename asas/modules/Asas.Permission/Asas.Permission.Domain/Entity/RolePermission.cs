using Asas.Core.EF;

namespace Asas.Permission.Domain.Entity
{
    public class RolePermission : AsasEntity<Guid>
    {
        public Guid RoleId { get; set; }
        public string PermissionName { get; set; } = default!;
        public bool IsGranted { get; set; } = true;
    }
}
