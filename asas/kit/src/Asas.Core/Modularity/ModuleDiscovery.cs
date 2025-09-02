// Asas.Core.Modularity/AsasModuleCatalog.cs
using System.Reflection;

namespace Asas.Core.Modularity;

internal sealed class AsasModuleCatalog
{
    public IReadOnlyList<AsasModule> ModulesInOrder { get; }
    public AsasModuleCatalog(IReadOnlyList<AsasModule> modules) => ModulesInOrder = modules;
}

internal static class ModuleDiscovery
{
    public static IReadOnlyList<Type> ResolveDependencyGraph(Type startupModule)
    {
        if (!typeof(AsasModule).IsAssignableFrom(startupModule))
            throw new InvalidOperationException($"{startupModule.Name} is not an AsasModule");

        var result = new List<Type>();
        var visited = new Dictionary<Type, int>(); // 0=visiting,1=done

        void Dfs(Type t)
        {
            if (visited.TryGetValue(t, out var state))
            {
                if (state == 0) throw new InvalidOperationException($"Cyclic module dependency around {t.FullName}");
                return;
            }
            visited[t] = 0;

            var deps = t.GetCustomAttribute<DependsOnAttribute>()?.DependedModuleTypes ?? Array.Empty<Type>();
            foreach (var d in deps)
            {
                if (!typeof(AsasModule).IsAssignableFrom(d))
                    throw new InvalidOperationException($"[DependsOn] target {d.FullName} is not an AsasModule");
                Dfs(d);
            }

            visited[t] = 1;
            result.Add(t);
        }

        Dfs(startupModule);
        return result; // deps come first, startup last
    }
}
