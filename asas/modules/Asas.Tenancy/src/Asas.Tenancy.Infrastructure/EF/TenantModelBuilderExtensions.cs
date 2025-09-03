using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Asas.Tenancy.Infrastructure.Runtime;

namespace Asas.Tenancy.Infrastructure.Ef;

internal static class TenantModelBuilderExtensions
{
    public static void ApplyTenancy(this ModelBuilder b, TenancyModelOptions opt)
    {
        var efProp = typeof(EF).GetMethod(nameof(EF.Property), BindingFlags.Public | BindingFlags.Static)!;
        var efPropString = efProp.MakeGenericMethod(typeof(string));
        var ambientProp = typeof(TenancyAmbient).GetProperty(nameof(TenancyAmbient.CurrentTenantId), BindingFlags.Static | BindingFlags.Public)!;

        foreach (var entity in b.Model.GetEntityTypes())
        {
            var clr = entity.ClrType;
            if (!ShouldScope(clr, opt)) continue;

            // Ensure TenantId property exists (shadow if needed)
            var prop = entity.FindProperty(opt.TenantIdPropertyName) ?? entity.AddProperty(opt.TenantIdPropertyName, typeof(string));
            prop.IsNullable = false;
            prop.SetMaxLength(opt.TenantIdMaxLength);
            entity.AddIndex(prop);

            // e => TenancyAmbient.CurrentTenantId == null || EF.Property<string>(e,"TenantId") == TenancyAmbient.CurrentTenantId
            var param = Expression.Parameter(clr, "e");
            var tenantIdAccess = Expression.Call(efPropString, param, Expression.Constant(opt.TenantIdPropertyName));
            var ambient = Expression.Property(null, ambientProp);
            var isNull = Expression.Equal(ambient, Expression.Constant(null, typeof(string)));
            var equals = Expression.Equal(tenantIdAccess, ambient);
            var body = Expression.OrElse(isNull, equals);
            var lambda = Expression.Lambda(body, param);

            entity.SetQueryFilter(lambda);
        }
    }

    internal static bool ShouldScope(Type clr, TenancyModelOptions opt)
    {
        if (opt.IsTenantScoped is not null) return opt.IsTenantScoped(clr);

        bool excludedNs = opt.ExcludeNamespaces.Any(ns => clr.Namespace?.StartsWith(ns, StringComparison.Ordinal) == true);
        if (excludedNs) return false;

        bool includedNs = opt.IncludeNamespaces.Count == 0 || opt.IncludeNamespaces.Any(ns => clr.Namespace?.StartsWith(ns, StringComparison.Ordinal) == true);
        if (!includedNs) return false;

        if (opt.ExcludeTypes.Contains(clr)) return false;
        if (opt.IncludeTypes.Contains(clr)) return true;

        return opt.ScopeAllByDefault;
    }
}
