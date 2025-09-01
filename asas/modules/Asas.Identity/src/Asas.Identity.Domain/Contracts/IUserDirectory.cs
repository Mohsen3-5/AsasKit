// Modules/Identity/AsasKit.Modules.Identity.Contracts/IUserDirectory.cs
namespace Asas.Identity.Domain.Contracts;

public interface IUserDirectory
{
    Task<UserView?> GetAsync(Guid id, Guid tenantId, CancellationToken ct = default);
    Task<UserView?> FindByEmailAsync(string email, Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<UserView>> SearchAsync(string term, Guid tenantId, int take = 20, CancellationToken ct = default);
}

public sealed record UserView(Guid Id, string Email, Guid TenantId, string? FullName);
