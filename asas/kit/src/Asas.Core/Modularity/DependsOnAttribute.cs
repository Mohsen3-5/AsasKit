// Asas.Core.Modularity/DependsOnAttribute.cs
namespace Asas.Core.Modularity;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class DependsOnAttribute : Attribute
{
    public DependsOnAttribute(params Type[] dependedModuleTypes) => DependedModuleTypes = dependedModuleTypes;
    public Type[] DependedModuleTypes { get; }
}
