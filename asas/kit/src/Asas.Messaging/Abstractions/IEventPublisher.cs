using Asas.Messaging.Domain;

namespace Asas.Messaging.Abstractions;
public interface IEventPublisher
{
    Task PublishDomainAsync<TEvent>(TEvent @event, CancellationToken ct = default) where TEvent : IDomainEvent;
}
