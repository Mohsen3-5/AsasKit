using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Options;

namespace Asas.Tenancy.Infrastructure.Ef;

public sealed class TenancyModelCustomizer : ModelCustomizer
{
    private readonly TenancyModelOptions _opt;

    public TenancyModelCustomizer(ModelCustomizerDependencies deps, IOptions<TenancyModelOptions> opt)
        : base(deps) => _opt = opt.Value;

    public override void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        base.Customize(modelBuilder, context);
        modelBuilder.ApplyTenancy(_opt);
    }
}
