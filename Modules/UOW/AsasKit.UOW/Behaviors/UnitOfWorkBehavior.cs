// AsasKit.UOW/Behaviors/UnitOfWorkBehavior.cs
using AsasKit.UOW.Abstractions;
using AsasKit.UOW.Options;
using MediatR;
using Microsoft.Extensions.Options;

namespace AsasKit.UOW.Behaviors;

public sealed class UnitOfWorkBehavior<TRequest, TResponse>(
    IUnitOfWork uow,
    IOptions<UowOptions> opt)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull 
{
    private readonly UowOptions _cfg = opt.Value;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        // skip TX for queries (by naming convention)
        if (_cfg.TreatRequestsEndingWithQueryAsReadOnly &&
            typeof(TRequest).Name.EndsWith("Query", StringComparison.Ordinal))
        {
            return await next();
        }

        // wrap handler in a single transaction and return result
        return await uow.ExecuteInTransactionAsync(
            async _ => await next(),
            _cfg.Isolation,
            ct);
    }
}
