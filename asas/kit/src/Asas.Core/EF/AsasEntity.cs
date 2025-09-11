namespace Asas.Core.EF
{
    public class AsasEntity<TId> : Entity<TId>
    {
        public Guid? TenantId { get; set; }
    }
}
