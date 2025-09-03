using Asas.Messaging.Domain;

namespace Asas.Core.Domain;
public abstract class AggregateRoot<TId>
{
    private readonly List<object> _events = new();
    public TId Id { get; protected set; } = default!;
    protected void Raise(object @event) => _events.Add(@event);
    public IReadOnlyCollection<object> DomainEvents => _events.AsReadOnly();
    public void ClearDomainEvents() => _events.Clear();
}