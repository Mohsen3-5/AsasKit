// AsasKit.Application/AppEventBase.cs
using AsasKit.Core;
using MediatR;

namespace AsasKit.Application;

public abstract record AppEventBase : IAppEvent, INotification
{
    public DateTime OccurredAtUtc { get; init; } = DateTime.UtcNow;
}

public sealed class ApplicationAssemblyMarker { }
