using FluentValidation;
using MediatR;
using ValidationException = GymManagement.Application.Common.Exceptions.ValidationException;

namespace GymManagement.Application.Common.Behaviours;

/// <summary>
/// Second behaviour in the pipeline.
/// Collects ALL validation failures before throwing — so the caller
/// sees every problem at once, not just the first one.
/// Throws ValidationException which the global error middleware maps to a 400.
/// Invalid data never reaches a handler.
/// </summary>
public class ValidationBehaviour<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
        => _validators = validators;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var failures = (await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, ct))))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count > 0)
            throw new ValidationException(failures);

        return await next();
    }
}
