namespace AsasKit.Application.Abstractions;
public interface ITenantProvider
{
    Guid CurrentTenantId { get; }
}