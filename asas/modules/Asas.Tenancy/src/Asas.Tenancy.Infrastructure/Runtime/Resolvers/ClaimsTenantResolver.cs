using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Asas.Tenancy.Infrastructure.Runtime.Resolvers;

public sealed class ClaimsTenantResolver(IHttpContextAccessor http, IOptions<TenancyOptions> opt) : ITenantResolver
{
    public Task<ResolveResult> ResolveAsync(CancellationToken ct = default)
    {
        var p = http.HttpContext?.User;
        if (p?.Identity?.IsAuthenticated == true)
        {
            var value = p.FindFirst(opt.Value.ClaimType)?.Value; // <- no extension needed
            if (!string.IsNullOrWhiteSpace(value)) return Task.FromResult(new ResolveResult { Id = value, Slug = value });
        }
        return Task.FromResult(new ResolveResult());
    }
}
