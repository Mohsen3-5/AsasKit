using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Asas.Tenancy.Infrastructure.Runtime.Resolvers;

public sealed class CompositeTenantResolver : ITenantResolver
{
    private readonly IReadOnlyList<ITenantResolver> _chain;

    public CompositeTenantResolver(IServiceProvider sp, IOptions<TenancyOptions> opt)
    {
        var order = opt.Value.ResolutionOrder.Select(s => s.Trim().ToLowerInvariant()).ToArray();
        var map = new Dictionary<string, ITenantResolver>
        {
            ["route"] = sp.GetRequiredService<RouteTenantResolver>(),
            ["header"] = sp.GetRequiredService<HeaderTenantResolver>(),
            ["claims"] = sp.GetRequiredService<ClaimsTenantResolver>(),
            ["subdomain"] = sp.GetRequiredService<SubdomainTenantResolver>(),
        };
        _chain = order.Where(map.ContainsKey).Select(k => map[k]).ToArray();
    }

    public async Task<ResolveResult> ResolveAsync(CancellationToken ct = default)
    {
        foreach (var r in _chain)
        {
            var v = await r.ResolveAsync(ct);
            if (v.HasValue) return v;
        }
        return new ResolveResult();
    }
}
