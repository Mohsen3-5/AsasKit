using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asas.Identity.Application.Contracts;
public interface IUserRoleService
{
    // If roles are tenant-agnostic, ignore tenantId in the impl.
    Task<IReadOnlyList<Guid>> GetRoleIdsAsync(Guid userId, Guid? tenantId = null, CancellationToken ct = default);
}