using Asas.Tenancy.Contracts;
using Asas.Tenancy.Infrastructure.EF;
using Microsoft.AspNetCore.Http;

namespace Asas.Tenancy.Application;
public sealed class HostCurrentTenant : ICurrentTenant
{
    private readonly IHttpContextAccessor _http;
    private readonly ITenantStore _store;
    private Guid? _tenantId;

    public HostCurrentTenant(IHttpContextAccessor http, ITenantStore store)
    {
        _http = http;
        _store = store;
    }

    public Guid Id
    {
        get
        {
            if (_tenantId.HasValue) return _tenantId.Value;

            var host = _http.HttpContext?.Request.Host.Host;
            var tenant = _store.FindByHost(host);
            _tenantId = tenant?.Id ?? Guid.Empty;
            return _tenantId.Value;
        }
    }

    public bool IsSet => Id != Guid.Empty;
}
