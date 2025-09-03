using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;  // <-- add this
using Microsoft.Extensions.Options;

namespace Asas.Tenancy.Infrastructure.Runtime.Resolvers;

public sealed class RouteTenantResolver(IHttpContextAccessor http, IOptions<TenancyOptions> opt) : ITenantResolver
{
    public Task<ResolveResult> ResolveAsync(CancellationToken ct = default)
    {
        var ctx = http.HttpContext;
        var routeData = ctx?.GetRouteData(); // works across ASP.NET Core versions

        if (routeData is not null &&
            routeData.Values.TryGetValue(opt.Value.RouteParamName, out var v) &&
            v is string s && !string.IsNullOrWhiteSpace(s))
        {
            return Task.FromResult(new ResolveResult { Slug = s });
        }

        return Task.FromResult(new ResolveResult());
    }
}
