using JetBrains.Annotations;

namespace Asas.Core.Modularity;

public interface IDependedTypesProvider
{
    [NotNull]
    Type[] GetDependedTypes();
}
