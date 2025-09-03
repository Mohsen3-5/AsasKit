using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Asas.Tenancy.Infrastructure.Runtime.Resolvers;

public sealed class HeaderTenantResolver(IHttpContextAccessor http, IOptions<TenancyOptions> opt) : ITenantResolver
{
    public Task<ResolveResult> ResolveAsync(CancellationToken ct = default)
    {
        var ctx = http.HttpContext;
        if (ctx?.Request.Headers.TryGetValue(opt.Value.HeaderName, out var h) == true)
        {
            var s = h.ToString();
            if (!string.IsNullOrWhiteSpace(s)) return Task.FromResult(new ResolveResult { Slug = s });
        }
        return Task.FromResult(new ResolveResult());
    }
}
