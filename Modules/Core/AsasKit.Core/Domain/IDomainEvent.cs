// Domain/IDomainEvent.cs
namespace AsasKit.Core;

/// <summary>Event that represents a fact that occurred within a domain aggregate.</summary>
public interface IDomainEvent
{
    DateTime OccurredAtUtc { get; }
}
