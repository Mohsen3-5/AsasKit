using Asas.Tenancy.Contracts;
using Microsoft.AspNetCore.Http;

namespace Asas.Tenancy.Application;
public sealed class HttpCurrentTenant : ICurrentTenant
{
    private readonly IHttpContextAccessor _http;
    public HttpCurrentTenant(IHttpContextAccessor http) => _http = http;

    public string? Id =>
        _http.HttpContext?.Items["TenantId"] as string
        ?? _http.HttpContext?.User.FindFirst("tenant_id")?.Value
        ?? _http.HttpContext?.Request.Headers["X-Tenant-Id"].FirstOrDefault();

    public bool IsSet => !string.IsNullOrWhiteSpace(Id);
}