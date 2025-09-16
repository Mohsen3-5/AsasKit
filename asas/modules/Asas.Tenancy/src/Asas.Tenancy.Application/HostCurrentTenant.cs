using Asas.Tenancy.Contracts;
using Microsoft.AspNetCore.Http;

namespace Asas.Tenancy.Application;
public sealed class HostCurrentTenant : ICurrentTenant
{
    private readonly IHttpContextAccessor _http;

    public HostCurrentTenant(IHttpContextAccessor http)
    {
        _http = http;
    }

    public Guid Id =>
        _http.HttpContext?.Items.TryGetValue("TenantId", out var v) == true && v is Guid g ? g : Guid.Empty;

    public bool IsSet => Id != Guid.Empty;
}
