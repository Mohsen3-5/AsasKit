using Asas.Permission.Infrastructure;

namespace Asas.Permission.Contracts
{
    public interface IPermissionDefinitionProvider
    {
        void Define(PermissionDefinitionContext ctx);
    }
}
