namespace AsasKit.Infrastructure.Data;
public sealed class TenantAccessor : ITenantAccessor
{
    private readonly Guid _tenantId;
    public TenantAccessor(Guid tenantId) => _tenantId = tenantId;
    public Guid CurrentTenantId => _tenantId;
}