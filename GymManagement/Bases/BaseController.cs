using GymManagement.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Web.Bases
{
    public abstract class BaseController : Controller
    {
        private IMediator? _mediator;

        /// <summary>Lazy-resolved MediatR — no constructor injection needed in every controller.</summary>
        protected IMediator Mediator =>
            _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();

        /// <summary>
        /// Maps Result ErrorType to the appropriate IActionResult.
        /// onSuccess delegate is only called when result.IsSuccess.
        /// </summary>
        protected IActionResult HandleResult<T>(
            Result<T> result,
            Func<T, IActionResult> onSuccess)
        {
            if (result.IsSuccess)
                return onSuccess(result.Value!);

            return result.ErrorType switch
            {
                ResultErrorType.NotFound => NotFound(),
                ResultErrorType.Forbidden => Forbid(),
                ResultErrorType.Conflict => BadRequestWithError(result.Error!),
                _ => BadRequestWithError(result.Error!)
            };
        }

        protected IActionResult HandleResult(
            Result result,
            Func<IActionResult> onSuccess)
        {
            if (result.IsSuccess)
                return onSuccess();

            return result.ErrorType switch
            {
                ResultErrorType.NotFound => NotFound(),
                ResultErrorType.Forbidden => Forbid(),
                _ => BadRequestWithError(result.Error!)
            };
        }

        /// <summary>
        /// Stores error in TempData and redirects — used for POST-Redirect-GET pattern.
        /// </summary>
        protected IActionResult RedirectWithError(string error, string action, object? routeValues = null)
        {
            TempData["Error"] = error;
            return routeValues is not null
                ? RedirectToAction(action, routeValues)
                : RedirectToAction(action);
        }

        protected IActionResult RedirectWithSuccess(string message, string action, object? routeValues = null)
        {
            TempData["Success"] = message;
            return routeValues is not null
                ? RedirectToAction(action, routeValues)
                : RedirectToAction(action);
        }

        private IActionResult BadRequestWithError(string error)
        {
            TempData["Error"] = error;
            return BadRequest(error);
        }
    }
}
