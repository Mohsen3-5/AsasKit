using Asas.Tenancy.Contracts;

namespace Asas.Tenancy.Infrastructure.Runtime;

public sealed class DefaultTenantAccessor : ITenantAccessor
{
    public TenantInfo? Current => TenantContextHolder.Current?.Tenant;
    public bool TryGet(out TenantInfo? tenant) { tenant = Current; return tenant is not null; }
}
