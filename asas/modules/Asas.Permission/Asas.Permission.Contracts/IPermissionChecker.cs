namespace Asas.Permission.Contracts;
public interface IPermissionChecker
{
    Task<bool> IsGrantedAsync(Guid userId, string permission, Guid? tenantId, CancellationToken ct = default);
}

