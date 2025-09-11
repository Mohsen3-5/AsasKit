

namespace Asas.Tenancy.Infrastructure.Runtime;
public static class TenancyAmbient
{
    public static Guid? CurrentTenantId => TenantContextHolder.Current?.Tenant?.Id;
}