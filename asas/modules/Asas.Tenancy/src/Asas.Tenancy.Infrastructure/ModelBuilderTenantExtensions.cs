using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Asas.Tenancy.Infrastructure;

public static class ModelBuilderTenantExtensions
{
    public static void ApplyTenantQueryFilters(this ModelBuilder b, DbContext ctx, ILogger? logger, params Type[] except)
    {
        var ctxType = ctx.GetType();
        var currentTenantIdProp = ctxType.GetProperty(nameof(TenancyDbContext.CurrentTenantId))!;
        var enabledProp = ctxType.GetProperty(nameof(TenancyDbContext.TenantFilterEnabled))!;
        logger.LogInformation(
                 "Tenant filter summary → applied: {CurrentTenantIdProp}",
                 currentTenantIdProp);
        foreach (var et in b.Model.GetEntityTypes())
        {
            var clr = et.ClrType;
            if (clr is null || et.IsKeyless) continue;
            if (except.Any(x => x.IsAssignableFrom(clr))) continue;

            var tenantProp = clr.GetProperty("TenantId");
            if (tenantProp is null) continue;

            // e =>
            var e = Expression.Parameter(clr, "e");
            var eTenant = Expression.Property(e, tenantProp);

            // ctx.CurrentTenantId / ctx.TenantFilterEnabled
            var ctxConst = Expression.Constant(ctx);
            var currentTenantId = Expression.Property(ctxConst, currentTenantIdProp);
            var enabledExpr = Expression.Property(ctxConst, enabledProp);
            var notEnabled = Expression.Not(enabledExpr);

            Expression body;
            if (tenantProp.PropertyType == typeof(Guid))
            {
                var eq = Expression.Equal(eTenant, currentTenantId);
                body = Expression.OrElse(notEnabled, eq);
            }
            else if (tenantProp.PropertyType == typeof(Guid?))
            {
                var nullGuid = Expression.Constant(null, typeof(Guid?));
                var isNull = Expression.Equal(eTenant, nullGuid);
                var asNullable = Expression.Convert(currentTenantId, typeof(Guid?));
                var eq = Expression.Equal(eTenant, asNullable);
                body = Expression.OrElse(notEnabled, Expression.OrElse(isNull, eq));
            }
            else continue;

            var lambda = Expression.Lambda(
                typeof(Func<,>).MakeGenericType(clr, typeof(bool)),
                body, e);

            b.Entity(clr).HasQueryFilter(lambda);
            logger.LogInformation(
    "Tenant filter summary → applied: ");
        }
    }
}
