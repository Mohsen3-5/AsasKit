// Entity.cs

using AsasKit.Domain.Common.DomainEvent;

namespace AsasKit.Domain.Common.Entity;
public abstract class Entity<TId>
{
    private readonly List<IDomainEvent> _events = new();
    public TId Id { get; protected set; } = default!;
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _events.AsReadOnly();
    protected void Raise(IDomainEvent @event) => _events.Add(@event);
    public void ClearDomainEvents() => _events.Clear();
}