using GymManagement.Application.Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace GymManagement.Web.Middleware
{
    /// <summary>
    /// Catches all unhandled exceptions in the pipeline.
    /// Maps to safe HTTP responses — never exposes stack traces or raw exception messages.
    /// Logs full exception details internally via Serilog.
    /// Must be registered FIRST in the middleware pipeline.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning("Validation failure on {Path}: {Errors}",
                    context.Request.Path,
                    string.Join("; ", ex.Errors.SelectMany(e => e.Value)));

                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Items["ValidationErrors"] = ex.Errors;
                await RenderErrorViewAsync(context, "ValidationError",
                    "Please correct the errors and try again.");
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning("Not found: {Message}", ex.Message);
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                await RenderErrorViewAsync(context, "NotFound",
                    "The resource you requested could not be found.");
            }
            catch (ForbiddenException ex)
            {
                _logger.LogWarning("Forbidden: {Message}", ex.Message);
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await RenderErrorViewAsync(context, "AccessDenied",
                    "You do not have permission to perform this action.");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex,
                    "Concurrency conflict on {Path}", context.Request.Path);

                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                await RenderErrorViewAsync(context, "Conflict",
                    "This record was modified by another user. Please refresh and try again.");
            }
            catch (Exception ex)
            {
                // Log full exception — user sees only a safe message
                _logger.LogError(ex,
                    "Unhandled exception on {Method} {Path}",
                    context.Request.Method, context.Request.Path);

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await RenderErrorViewAsync(context, "Error",
                    "An unexpected error occurred. Please try again later.");
            }
        }

        private static async Task RenderErrorViewAsync(
            HttpContext context,
            string viewName,
            string message)
        {
            // Store message for the error view to pick up
            context.Items["ErrorMessage"] = message;

            // Redirect to the error controller which renders the correct view
            context.Request.Path = $"/Home/Error/{viewName}";

            // If headers already sent we cannot redirect — just write a plain response
            if (!context.Response.HasStarted)
            {
                context.Response.ContentType = "text/html";
                await context.Response.WriteAsync(
                    $"<html><body><h2>{message}</h2></body></html>");
            }
        }
    }
}