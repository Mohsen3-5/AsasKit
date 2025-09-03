using System.Diagnostics;
using Asas.Tenancy.Contracts;
using Microsoft.AspNetCore.Http;

namespace Asas.Tenancy.Infrastructure.Runtime;

public sealed class TenantResolutionMiddleware(RequestDelegate next, ITenantResolver resolver)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            var rr = await resolver.ResolveAsync(context.RequestAborted);
            var id = rr.Id ?? rr.Slug; // we filter by Id; fall back to slug
            TenantInfo? tenant = string.IsNullOrWhiteSpace(id) ? null : new TenantInfo { Id = id!, Slug = rr.Slug };

            TenantContextHolder.Current = new TenantContext(tenant);

            var act = Activity.Current;
            if (tenant is not null && act is not null)
            {
                act.SetTag("tenant.id", tenant.Id);
                if (!string.IsNullOrWhiteSpace(tenant.Slug)) act.SetTag("tenant.slug", tenant.Slug);
            }

            await next(context);
        }
        finally
        {
            TenantContextHolder.Current = null;
        }
    }
}
