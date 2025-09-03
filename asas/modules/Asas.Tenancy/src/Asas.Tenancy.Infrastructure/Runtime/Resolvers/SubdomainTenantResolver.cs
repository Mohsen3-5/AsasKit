using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Asas.Tenancy.Infrastructure.Runtime.Resolvers;

public sealed class SubdomainTenantResolver(IHttpContextAccessor http, IOptions<TenancyOptions> opt) : ITenantResolver
{
    public Task<ResolveResult> ResolveAsync(CancellationToken ct = default)
    {
        var host = http.HttpContext?.Request.Host.Host;
        if (string.IsNullOrWhiteSpace(host)) return Task.FromResult(new ResolveResult());

        var root = opt.Value.RootDomain;
        string? sub = null;

        if (!string.IsNullOrWhiteSpace(root) && host.EndsWith(root, StringComparison.OrdinalIgnoreCase))
            sub = host[..^root.Length].TrimEnd('.');
        else
        {
            var parts = host.Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2) sub = parts[0];
        }

        return Task.FromResult(string.IsNullOrWhiteSpace(sub) ? new ResolveResult() : new ResolveResult { Slug = sub });
    }
}
