// Abstractions/IEventPublisher.cs

// Abstractions/IEventPublisher.cs
using AsasKit.Shared.Messaging.Domain;

namespace AsasKit.Shared.Messaging.Abstractions;

/// <summary>Publishes events to in-proc handlers and/or external transports (via infra adapters).</summary>
public interface IEventPublisher
{
    Task PublishDomainAsync<TEvent>(TEvent @event, CancellationToken ct = default) where TEvent : IDomainEvent ;
}
