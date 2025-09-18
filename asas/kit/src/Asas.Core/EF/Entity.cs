namespace Asas.Core.EF;
public abstract class AsasEntity<TId> : Entity
{
    public TId Id { get; protected set; } = default!;
}

public abstract class Entity
{
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public int? TenantId { get; set; }
}