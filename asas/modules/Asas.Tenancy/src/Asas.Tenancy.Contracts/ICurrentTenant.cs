namespace Asas.Tenancy.Contracts;
public interface ICurrentTenant
{
    Guid Id { get; }
    bool IsSet { get; }
}