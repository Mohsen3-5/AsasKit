using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kernel;

// ---------- Attributes & Contracts ----------

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class DependsOnAttribute : Attribute
{
    public DependsOnAttribute(Type moduleType) => ModuleType = moduleType;
    public Type ModuleType { get; }
}

// Choose class (with virtuals) so implementors can override only what they need
public abstract class AsasModule
{
    public virtual void PreConfigureServices(IServiceCollection services) { }
    public virtual void ConfigureServices(IServiceCollection services, IConfiguration cfg) { }
    public virtual void OnApplicationInitialization(IApplicationBuilder app) { }
}

// ---------- Runner ----------

public static class ModuleRunner
{
    /// <summary>
    /// Discovers dependencies (via [DependsOn]) and wires modules.
    /// Returns the built WebApplication.
    /// </summary>
    public static WebApplication Build(WebApplicationBuilder builder, params Type[] startupModules)
    {
        var allModuleTypes = ResolveGraph(startupModules);
        var instances = allModuleTypes.Select(t => (AsasModule)Activator.CreateInstance(t)!).ToList();

        // 1) PreConfigure
        foreach (var m in instances)
            m.PreConfigureServices(builder.Services);

        // 2) Configure
        foreach (var m in instances)
            m.ConfigureServices(builder.Services, builder.Configuration);

        // 3) Build & Initialize
        var app = builder.Build();
        foreach (var m in instances)
            m.OnApplicationInitialization(app);

        return app;
    }

    // Topological order: dependencies first
    private static IReadOnlyList<Type> ResolveGraph(IEnumerable<Type> roots)
    {
        var result = new List<Type>();
        var visiting = new HashSet<Type>();
        var visited = new HashSet<Type>();

        void Visit(Type t)
        {
            if (visited.Contains(t)) return;
            if (!visiting.Add(t))
                throw new InvalidOperationException($"Cyclic module dependency detected involving {t.FullName}");

            var deps = t.GetCustomAttributes<DependsOnAttribute>()
                        .Select(a => a.ModuleType);

            foreach (var d in deps) Visit(d);

            visiting.Remove(t);
            visited.Add(t);
            result.Add(t);
        }

        foreach (var r in roots) Visit(r);
        return result;
    }
}
