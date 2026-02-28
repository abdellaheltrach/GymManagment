
using GymManagement.Domain.Interfaces;
using GymManagement.Infrastructure.Context;
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
    private readonly AppDbContext _context;
    private readonly ILogger<TransactionBehaviour<TRequest, TResponse>> _logger;

    public TransactionBehaviour(
        IUnitOfWork uow,
        ILogger<TransactionBehaviour<TRequest, TResponse>> logger,
        AppDbContext context)
    {
        _uow = uow;
        _logger = logger;
        _context = context;
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

        var strategy = _context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async cancellationToken =>
        {
            await using var tx = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                response = await next();
                await _context.SaveChangesAsync(cancellationToken);
                await tx.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Rolled back transaction for {RequestName}", requestName);

                throw;
            }
        }, ct);

        return response;
    }

}
