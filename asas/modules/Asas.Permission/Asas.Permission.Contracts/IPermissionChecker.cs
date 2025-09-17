namespace Asas.Permission.Contracts;
public interface IPermissionChecker
{
    Task<bool> IsGrantedAsync(Guid userId, string permission, int? tenantId, CancellationToken ct = default);
}

