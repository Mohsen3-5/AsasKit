using MediatR;

namespace Asas.Messaging.Domain;
/// <summary>Event that represents a fact that occurred within a domain aggregate.</summary>
public interface IDomainEvent : INotification
{
    DateTime OccurredAtUtc { get; }
}
