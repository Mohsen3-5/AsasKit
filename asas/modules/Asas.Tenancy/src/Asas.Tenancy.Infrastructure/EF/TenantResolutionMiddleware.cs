using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Asas.Tenancy.Infrastructure.Runtime;

namespace Asas.Tenancy.Infrastructure.EF;
public sealed class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _log;
    private readonly TenancyOptions _options;

    public TenantResolutionMiddleware(
        RequestDelegate next,
        ILogger<TenantResolutionMiddleware> log,
        IOptions<TenancyOptions> options)
        => (_next, _log, _options) = (next, log, options.Value);

    public async Task Invoke(HttpContext ctx, ITenantStore store)
    {
        // resolution logic
        var host = ctx.Request.Host.Host;
        _log.LogWarning("host : {host}", host);

        string? fromSub = null;
        if (!string.IsNullOrWhiteSpace(host))
        {
            fromSub = host.Split('.')[0];
        }
        _log.LogWarning("sub : {fromSub}", fromSub);

        TenantDto? tenant = null;
        if (!string.IsNullOrWhiteSpace(fromSub))
            tenant = await store.FindByHostAsync(fromSub);

        // Fallback logic
        if (tenant is null && _options.FallbackTenantId.HasValue)
        {
            _log.LogInformation("Tenancy: Falling back to '{FallbackId}'", _options.FallbackTenantId);
            tenant = await store.FindByIdAsync(_options.FallbackTenantId.Value);
        }

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

        ctx.Items["TenantId"] = tenant.Id;
        ctx.Items["Tenant"] = tenant;

        _log.LogInformation("Tenancy: resolved & verified tenant '{TenantId}' ({Name})", tenant.Id, tenant.Name);
        await _next(ctx);
    }
}
