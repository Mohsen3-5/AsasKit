using Asas.Tenancy.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Asas.Tenancy.Application;

public sealed class HostCurrentTenant : ICurrentTenant
{
    private readonly IHttpContextAccessor _http;
    private readonly ILogger<HostCurrentTenant>? _log;

    public HostCurrentTenant(IHttpContextAccessor http, ILogger<HostCurrentTenant>? log)
    {
        _http = http;
        _log = log;
    }

    public int Id
    {
        get
        {
            if (_http.HttpContext?.Items.TryGetValue("TenantId", out var v) == true && v is int i)
            {
                return i;
            }

            return 0;
        }
    }

    public bool IsSet => Id > 0;
}
