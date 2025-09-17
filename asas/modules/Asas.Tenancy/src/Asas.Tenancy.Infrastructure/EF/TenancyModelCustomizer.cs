using Asas.Tenancy.Contracts;
using Asas.Tenancy.Infrastructure.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public sealed class TenancyModelCustomizer : ModelCustomizer
{
    private readonly TenancyModelOptions _opt;
    private readonly ICurrentTenant _current;
    private readonly ILogger<TenancyModelCustomizer>? _log;

    public TenancyModelCustomizer(
        ModelCustomizerDependencies deps,
        IOptions<TenancyModelOptions>? opt = null,          // <- optional
        ICurrentTenant? current = null,                     // <- optional
        ILogger<TenancyModelCustomizer>? log = null)        // <- optional
        : base(deps)
    {
        _opt = opt?.Value ?? new TenancyModelOptions();     // default options for design-time
        _current = current ?? NullCurrentTenant.Instance;   // no tenant at design-time
        _log = log;
    }

    public override void Customize(ModelBuilder b, DbContext context)
    {
        base.Customize(b, context);
        _log?.LogInformation("Tenancy: building model for {Context} (Prop={Prop}, ScopeAll={Scope})",
            context.GetType().Name, _opt.TenantIdPropertyName, _opt.ScopeAllByDefault);

        var applied = TenantModelBuilderExtensions.ApplyTenancy(
            b, context, _opt, () => _current.Id, _log);

        _log?.LogInformation("Tenancy: global query filter applied to {Count} entity types.", applied);
    }

    private sealed class NullCurrentTenant : ICurrentTenant
    {
        public static readonly NullCurrentTenant Instance = new();
        public int? Id => null;
        public bool IsSet => false;

    }
}
