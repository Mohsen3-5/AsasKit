public interface ITenantConnectionResolver
{
    string GetConnectionString(Guid tenantId);
}