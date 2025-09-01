namespace Asas.Messaging.Domain;
public interface IAggregateRoot
{
    IReadOnlyCollection<IDomainEvent> DequeueDomainEvents();
}

public abstract class AggregateRoot : IAggregateRoot
{
    private readonly List<IDomainEvent> _domain = new();

    protected void Raise(IDomainEvent e) => _domain.Add(e);

    public IReadOnlyCollection<IDomainEvent> DequeueDomainEvents() { var c = _domain.ToArray(); _domain.Clear(); return c; }
}
