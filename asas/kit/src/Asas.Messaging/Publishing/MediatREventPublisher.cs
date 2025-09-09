
using Asas.Messaging.Abstractions;
using Asas.Messaging.Domain;
using MediatR;

namespace Asas.Messaging.Publishing;
public sealed class MediatREventPublisher(IMediator mediator) : IEventPublisher
{
    public Task PublishDomainAsync<TEvent>(TEvent e, CancellationToken ct = default)
        where TEvent : IDomainEvent
        => mediator.Publish(e, ct);
}
