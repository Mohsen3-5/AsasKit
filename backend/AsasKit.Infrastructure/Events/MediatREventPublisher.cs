using AsasKit.Core;
using MediatR;
using IDomainEvent = AsasKit.Core.IDomainEvent;

public sealed class MediatREventPublisher(IMediator mediator) : IEventPublisher
{
    public Task PublishDomainAsync<T>(T e, CancellationToken ct = default) where T : IDomainEvent
        => mediator.Publish(e, ct);

    public Task PublishAppAsync<T>(T e, CancellationToken ct = default) where T : IAppEvent
        => mediator.Publish(e, ct);

    public Task PublishIntegrationAsync<T>(T e, CancellationToken ct = default) where T : IIntegrationEvent
        => throw new NotImplementedException("Integration events go through the Outbox");
}
