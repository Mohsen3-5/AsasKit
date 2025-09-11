
using Asas.Permission.Domain.Entity;

namespace Asas.Permission.Contracts
{
    public interface IPermissionDefinitionStore
    {
        Task<IReadOnlyList<AsasPermission>> GetAllAsync(CancellationToken ct = default);
    }
}
