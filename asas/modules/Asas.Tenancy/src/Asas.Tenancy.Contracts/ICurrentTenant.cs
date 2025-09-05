namespace Asas.Tenancy.Contracts;
public interface ICurrentTenant
{
    string? Id { get; }
    bool IsSet { get; }
}