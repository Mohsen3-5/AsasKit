using MediatR;
using AsasKit.Shared.Messaging.Abstractions;
using AsasKit.Shared.Messaging.Domain;

namespace AsasKit.Shared.Messaging.Publishing;

public sealed class MediatREventPublisher(IMediator mediator) : IEventPublisher
{
    public Task PublishDomainAsync<TEvent>(TEvent e, CancellationToken ct = default)
        where TEvent : IDomainEvent
        => mediator.Publish(e, ct);
}
