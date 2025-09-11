
using Asas.Permission.Domain.Entity;
using Asas.Permission.Contracts;
using Asas.Permission.Infrastructure;
namespace Asas.Permission.Application
{
    public sealed class PermissionDefinitionStore : IPermissionDefinitionStore
    {
        private readonly IEnumerable<IPermissionDefinitionProvider> _providers;
        public PermissionDefinitionStore(IEnumerable<IPermissionDefinitionProvider> providers) => _providers = providers;

        public Task<IReadOnlyList<AsasPermission>> GetAllAsync(CancellationToken ct = default)
        {
            var ctx = new PermissionDefinitionContext();
            foreach (var p in _providers) p.Define(ctx);
            return Task.FromResult((IReadOnlyList<AsasPermission>)ctx.Items);
        }
    }
}
