using FluentValidation;
using MediatR;

namespace AsasKit.Application.Behaviors;
public sealed class ValidationBehavior<TReq,TRes> : IPipelineBehavior<TReq,TRes> where TReq :  IRequest<TRes>
{
    private readonly IEnumerable<IValidator<TReq>> _validators;
    public ValidationBehavior(IEnumerable<IValidator<TReq>> validators) => _validators = validators;

    public async Task<TRes> Handle(TReq request, RequestHandlerDelegate<TRes> next, CancellationToken ct)
    {
        if (_validators.Any())
        {
            var ctx = new ValidationContext<TReq>(request);
            var errors = (await Task.WhenAll(_validators.Select(v => v.ValidateAsync(ctx, ct))))
                .SelectMany(r => r.Errors).Where(f => f != null).ToList();
            if (errors.Count != 0)
                throw new ValidationException(errors);
        }
        return await next();
    }
}