// Domain/IAppEvent.cs
namespace AsasKit.Core;

/// <summary>In-process application event, used to decouple modules within the same process.</summary>
public interface IAppEvent : IDomainEvent
{
}
