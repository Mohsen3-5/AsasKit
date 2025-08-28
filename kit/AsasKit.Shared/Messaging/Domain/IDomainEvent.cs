// Domain/IDomainEvent.cs
using MediatR;

namespace AsasKit.Core.Domain;

/// <summary>Event that represents a fact that occurred within a domain aggregate.</summary>
public interface IDomainEvent : INotification
{
    DateTime OccurredAtUtc { get; }
}
