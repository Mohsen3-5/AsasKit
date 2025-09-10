
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
namespace Asas.Tenancy.Infrastructure.EF;
public sealed class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _log;
    public TenantResolutionMiddleware(RequestDelegate next, ILogger<TenantResolutionMiddleware> log)
        => (_next, _log) = (next, log);

    public async Task Invoke(HttpContext ctx, ITenantStore store)
    {

        // TODO: support subdomain resolution strategie;
        var host = ctx.Request.Host.Host;
        var fromHeader = ctx.Request.Headers["X-Tenant-Id"].FirstOrDefault();

        string? fromSub = null;
        if (!string.IsNullOrWhiteSpace(host) &&
            !host.Equals("localhost", StringComparison.OrdinalIgnoreCase) &&
            !host.Equals("127.0.0.1") &&
            host.Contains('.')) // real subdomain like foo.example.com
        {
            fromSub = host.Split('.')[0];
        }else if (!string.IsNullOrWhiteSpace(fromHeader))
        {
            fromSub = fromHeader;
        }
        _log.LogWarning("sub : {fromSub}", fromSub);

        // ignore "localhost" as tenant
        TenantDto? tenant = null;
        Guid guid;
        if (!string.IsNullOrWhiteSpace(fromSub))
            tenant = await store.FindByHostAsync(fromSub);
        else if (!string.IsNullOrWhiteSpace(fromHeader) && Guid.TryParse(fromHeader, out guid))
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
