namespace Asas.Messaging.Domain;

/// <summary>
/// Minimal specification contract. Infrastructure can adapt it to DB queries; in-memory checks can use IsSatisfiedBy.
/// </summary>
public interface ISpecification<in T>
{
    bool IsSatisfiedBy(T entity);
}
