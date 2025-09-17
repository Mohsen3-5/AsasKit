// Modules/Identity/AsasKit.Modules.Identity.Contracts/IUserDirectory.cs
namespace Asas.Identity.Domain.Contracts;

public interface IUserDirectory
{
    Task<UserView?> GetAsync(Guid id, CancellationToken ct = default);
    Task<UserView?> FindByEmailAsync(string email, CancellationToken ct = default);
    Task<IReadOnlyList<UserView>> SearchAsync(string term, int take = 20, CancellationToken ct = default);
}

public sealed record UserView(Guid Id, string Email, string? FullName);
