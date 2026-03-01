
using GymManagement.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GymManagement.Application.Common.Behaviours;

/// <summary>
/// Third behaviour in the pipeline.
/// Only activates for requests that implement ICommandBase.
/// Wraps handler in a DB transaction — commits on success, rolls back on exception.
/// Queries do not implement ICommandBase and pass straight through.
/// </summary>
public class TransactionBehaviour<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<TransactionBehaviour<TRequest, TResponse>> _logger;

    public TransactionBehaviour(
        IUnitOfWork uow,
        ILogger<TransactionBehaviour<TRequest, TResponse>> logger)
    {
        _uow    = uow;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (request is not ICommandBase)
            return await next();

        var requestName = typeof(TRequest).Name;
        TResponse response = default!;

        await _uow.ExecuteInTransactionAsync(async cancellationToken =>
        {
            response = await next();
        }, ct);

        return response;
    }
}
