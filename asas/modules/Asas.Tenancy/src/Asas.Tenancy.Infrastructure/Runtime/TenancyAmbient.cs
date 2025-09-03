

namespace Asas.Tenancy.Infrastructure.Runtime;
public static class TenancyAmbient
{
    public static string? CurrentTenantId => TenantContextHolder.Current?.Tenant?.Id;
}