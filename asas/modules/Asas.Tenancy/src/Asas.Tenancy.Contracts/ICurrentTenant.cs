namespace Asas.Tenancy.Contracts;
public interface ICurrentTenant
{
    int? Id { get; }
    bool IsSet { get; }
}