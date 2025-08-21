namespace AsasKit.Domain.Common.DomainEvent;
public abstract class DomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}