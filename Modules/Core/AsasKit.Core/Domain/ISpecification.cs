// Domain/ISpecification.cs
namespace AsasKit.Core;

/// <summary>
/// Minimal specification contract. Infrastructure can adapt it to DB queries; in-memory checks can use IsSatisfiedBy.
/// </summary>
public interface ISpecification<in T>
{
    bool IsSatisfiedBy(T entity);
}
