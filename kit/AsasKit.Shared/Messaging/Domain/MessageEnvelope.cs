// Domain/MessageEnvelope.cs
namespace AsasKit.Shared.Messaging.Domain;

/// <summary>Transport envelope for messages published to a broker or persisted in an outbox.</summary>
public sealed record MessageEnvelope<T>(
    T Payload,
    Guid MessageId,
    string Name,
    string Version,
    string? CorrelationId,
    string? CausationId,
    DateTime OccurredAtUtc,
    IReadOnlyDictionary<string, string>? Headers
);
