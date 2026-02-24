using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace GymManagement.Application.Common.Behaviours;

/// <summary>
/// First behaviour in the pipeline.
/// Logs request start, completion, and duration for every MediatR request.
/// Long-running requests (over 3 seconds) are logged as warnings.
/// </summary>
public class LoggingBehaviour<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehaviour<TRequest, TResponse>> _logger;

    public LoggingBehaviour(ILogger<LoggingBehaviour<TRequest, TResponse>> logger)
        => _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var requestName = typeof(TRequest).Name;
        var sw          = Stopwatch.StartNew();

        _logger.LogInformation("Handling {RequestName}", requestName);

        try
        {
            var response = await next();
            sw.Stop();

            if (sw.ElapsedMilliseconds > 3000)
                _logger.LogWarning(
                    "Slow request {RequestName} completed in {ElapsedMs}ms",
                    requestName, sw.ElapsedMilliseconds);
            else
                _logger.LogInformation(
                    "Handled {RequestName} in {ElapsedMs}ms",
                    requestName, sw.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex,
                "Request {RequestName} failed after {ElapsedMs}ms",
                requestName, sw.ElapsedMilliseconds);
            throw;
        }
    }
}
