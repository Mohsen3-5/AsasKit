namespace Asas.Tenancy.Contracts;
public interface ITenantAccessor
{
    TenantInfo? Current { get; }
    bool TryGet(out TenantInfo? tenant);
}