using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asas.Core.EF;

namespace Asas.Permission.Domain.Entity
{
    public class UserPermissionOverride : AsasEntity<Guid>
    {
        public Guid UserId { get; set; }
        public string PermissionName { get; set; } = default!;
        public bool? IsGranted { get; set; }
    }
}
