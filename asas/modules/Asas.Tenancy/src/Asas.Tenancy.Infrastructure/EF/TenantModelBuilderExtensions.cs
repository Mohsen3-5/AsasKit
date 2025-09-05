using System.Linq.Expressions;
using Asas.Tenancy.Infrastructure.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Asas.Tenancy.Infrastructure.EF;

internal static class TenantModelBuilderExtensions
{
    public static int ApplyTenancy(
        this ModelBuilder b,
        DbContext ctx,
        TenancyModelOptions opt,
        Func<string?> tenantAccessor,
        ILogger? log = null)
    {
        var propName = opt.TenantIdPropertyName;
        var count = 0;

        foreach (var et in b.Model.GetEntityTypes())
        {
            var clr = et.ClrType;
            if (!IsTenantScoped(clr, opt)) { log?.LogTrace("Tenancy: skip {Entity} (excluded)", clr.Name); continue; }

            var p = et.FindProperty(propName);
            if (p is null || p.ClrType != typeof(string))
            {
                log?.LogTrace("Tenancy: skip {Entity} (no string {Prop})", clr.Name, propName);
                continue;
            }

            b.Entity(clr).Property<string>(propName).HasMaxLength(opt.TenantIdMaxLength);
            b.Entity(clr).HasIndex(propName);

            var param = Expression.Parameter(clr, "e");
            var prop = Expression.Call(
              typeof(Microsoft.EntityFrameworkCore.EF),
              nameof(Microsoft.EntityFrameworkCore.EF.Property),
              new[] { typeof(string) },
              param,
              Expression.Constant(propName));

            var tenantIdGetter = Expression.Property(
                Expression.Constant(new ContextTenantAccessor(ctx, tenantAccessor)),
                nameof(ContextTenantAccessor.TenantId));

            var body = Expression.Equal(prop, tenantIdGetter);
            var lambda = Expression.Lambda(body, param);

            b.Entity(clr).HasQueryFilter(lambda);
            count++;
            log?.LogDebug("Tenancy: applied filter to {Entity}", clr.Name);
        }
        return count;
    }

    private static bool IsTenantScoped(Type t, TenancyModelOptions opt)
    {
        if (opt.IsTenantScoped is not null) return opt.IsTenantScoped(t);

        var ns = t.Namespace ?? "";
        if (opt.ExcludeNamespaces.Any(ns.StartsWith)) return false;
        if (opt.IncludeTypes.Contains(t)) return true;
        if (opt.ExcludeTypes.Contains(t)) return false;

        return opt.ScopeAllByDefault && (opt.IncludeNamespaces.Count == 0 || opt.IncludeNamespaces.Any(ns.StartsWith));
    }

    private sealed class ContextTenantAccessor
    {
        private readonly DbContext _ctx;
        private readonly Func<string?> _get;
        public ContextTenantAccessor(DbContext ctx, Func<string?> get) { _ctx = ctx; _get = get; }
        public string? TenantId => _get(); // evaluated per query execution
    }
}
