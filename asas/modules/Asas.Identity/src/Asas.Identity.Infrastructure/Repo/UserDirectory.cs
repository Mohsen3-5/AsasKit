// Modules/Identity/AsasKit.Modules.Identity/UserDirectory.cs

using Asas.Identity.Domain.Contracts;
using Asas.Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Asas.Identity.Infrastructure.Repo;

public sealed class UserDirectory(UserManager<AsasUser> users) : IUserDirectory
{
    public async Task<UserView?> GetAsync(Guid id, CancellationToken ct = default)
    {
        var u = await users.Users
            .AsNoTracking()
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync(ct);

        return u is null ? null : Map(u);
    }

    public async Task<UserView?> FindByEmailAsync(string email, CancellationToken ct = default)
    {
        var norm = users.NormalizeEmail(email);
        var u = await users.Users
            .AsNoTracking()
            .Where(x => x.NormalizedEmail == norm)
            .FirstOrDefaultAsync(ct);

        return u is null ? null : Map(u);
    }

    public async Task<IReadOnlyList<UserView>> SearchAsync(
        string term, int take = 20, CancellationToken ct = default)
    {
        term = term?.Trim() ?? string.Empty;
        take = take <= 0 ? 20 : take;

        var q = users.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(term))
        {
            var norm = users.NormalizeEmail(term);
            q = q.Where(x =>
                (x.Email ?? "").Contains(term) ||
                (x.UserName ?? "").Contains(term) ||
                x.NormalizedEmail == norm);
        }

        var list = await q.OrderBy(x => x.Email).Take(take).ToListAsync(ct);
        return list.Select(Map).ToList();
    }

    // Map to the contract type; we try to read optional custom fields safely.
    private static UserView Map(AsasUser u)
        => new(
            u.Id,
            u.Email ?? string.Empty,
            TryGetOptionalString(u, "FullName"));

    private static string? TryGetOptionalString(object obj, string prop)
        => obj.GetType().GetProperty(prop)?.GetValue(obj) as string;
}
