using Asas.Permission.Application;
namespace Asas.Permission.Contracts
{
    public interface IPermissionDefinitionProvider
    {
        void Define(PermissionDefinitionContext ctx);
    }
}
