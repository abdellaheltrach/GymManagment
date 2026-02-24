using GymManagement.Application.Common.Models;
using GymManagement.Domain.Interfaces;
using MediatR;
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
        _logger.LogDebug("Beginning transaction for {RequestName}", requestName);

        await _uow.BeginTransactionAsync(ct);
        try
        {
            var response = await next();
            await _uow.CommitTransactionAsync(ct);
            _logger.LogDebug("Committed transaction for {RequestName}", requestName);
            return response;
        }
        catch (Exception ex)
        {
            await _uow.RollbackTransactionAsync(ct);
            _logger.LogError(ex, "Rolled back transaction for {RequestName}", requestName);
            throw;
        }
    }
}
