// Abstractions/IEventPublisher.cs
namespace AsasKit.Core;

/// <summary>Publishes events to in-proc handlers and/or external transports (via infra adapters).</summary>
public interface IEventPublisher
{
    Task PublishDomainAsync<T>(T @event, CancellationToken ct = default) where T : IDomainEvent;
    Task PublishAppAsync<T>(T @event, CancellationToken ct = default) where T : IAppEvent;
    Task PublishIntegrationAsync<T>(T @event, CancellationToken ct = default) where T : IIntegrationEvent;
}
