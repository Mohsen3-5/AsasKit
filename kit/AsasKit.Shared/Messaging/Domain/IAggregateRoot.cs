// AsasKit.Core.Domain
using AsasKit.Shared.Messaging.Domain;

public interface IAggregateRoot
{
    IReadOnlyCollection<IDomainEvent> DequeueDomainEvents();
    IReadOnlyCollection<IIntegrationEvent> DequeueIntegrationEvents();
}

public abstract class AggregateRoot : IAggregateRoot
{
    private readonly List<IDomainEvent> _domain = new();
    private readonly List<IIntegrationEvent> _integration = new();

    protected void Raise(IDomainEvent e) => _domain.Add(e);
    protected void Publish(IIntegrationEvent e) => _integration.Add(e);

    public IReadOnlyCollection<IDomainEvent> DequeueDomainEvents() { var c = _domain.ToArray(); _domain.Clear(); return c; }
    public IReadOnlyCollection<IIntegrationEvent> DequeueIntegrationEvents() { var c = _integration.ToArray(); _integration.Clear(); return c; }
}
