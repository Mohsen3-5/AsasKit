
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
namespace Asas.Tenancy.Infrastructure.EF;
public sealed class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _log;
    public TenantResolutionMiddleware(RequestDelegate next, ILogger<TenantResolutionMiddleware> log)
        => (_next, _log) = (next, log);

    public async Task Invoke(HttpContext ctx, ITenantStore store)
    {
        var fromHeader = ctx.Request.Headers["X-Tenant-Id"].FirstOrDefault();
        _log.LogWarning("fromHeader : {Hdr}", fromHeader);
        var fromClaim = ctx.User.FindFirst("tenant_id")?.Value;
        _log.LogWarning("fromClaim {Claim}", fromClaim);

        // ignore "localhost" as tenant


        TenantDto? tenant = null;
        var candidate = fromHeader ?? fromClaim;

        Guid guid;
        if (!string.IsNullOrWhiteSpace(candidate) && Guid.TryParse(candidate, out guid))
            tenant = await store.FindByIdAsync(guid);
        _log.LogWarning("Tenancy: {tenant}",
              tenant);
        if (tenant is null)
        {
            ctx.Response.StatusCode = StatusCodes.Status404NotFound;
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                error = "UnknownTenant",
                message = "Tenant not found."
            }));
            return;
        }

        ctx.Items["TenantId"] = tenant.Id;   // this is the slug from Tenants.Identifier
        ctx.Items["Tenant"] = tenant;

        _log.LogInformation("Tenancy: resolved & verified tenant '{TenantId}' ({Name})", tenant.Id, tenant.Name);
        await _next(ctx);
    }
}
