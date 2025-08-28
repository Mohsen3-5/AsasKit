// Domain/IIntegrationEvent.cs
namespace AsasKit.Shared.Messaging.Domain;

/// <summary>Cross-process message contract. Must be versioned and idempotent.</summary>
public interface IIntegrationEvent
{
    Guid Id { get; }
    DateTime OccurredAtUtc { get; }
    string Version { get; }
}
