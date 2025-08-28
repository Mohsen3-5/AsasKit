using MediatR;
using AsasKit.Core.Abstractions;
using AsasKit.Core.Domain;

namespace AsasKit.Shared.Messaging.Publishing;

public sealed class MediatREventPublisher(IMediator mediator) : IEventPublisher
{
    public Task PublishDomainAsync<TEvent>(TEvent e, CancellationToken ct = default)
        where TEvent : IDomainEvent
        => mediator.Publish(e, ct);
}
