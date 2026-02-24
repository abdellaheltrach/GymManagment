using GymManagement.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Web.Extensions
{
    public static class ResultExtensions
    {
        /// <summary>
        /// Maps a Result to TempData error message so the next view can display it.
        /// Usage in controller: if (result.IsFailure) return this.RedirectWithError(result, nameof(Index));
        /// </summary>
        public static IActionResult RedirectWithError<T>(
            this Controller controller,
            Result<T> result,
            string actionName)
        {
            controller.TempData["Error"] = result.Error;
            return controller.RedirectToAction(actionName);
        }

        public static IActionResult RedirectWithError(
            this Controller controller,
            Result result,
            string actionName)
        {
            controller.TempData["Error"] = result.Error;
            return controller.RedirectToAction(actionName);
        }

        public static IActionResult RedirectWithSuccess(
            this Controller controller,
            string message,
            string actionName,
            object? routeValues = null)
        {
            controller.TempData["Success"] = message;
            return routeValues is not null
                ? controller.RedirectToAction(actionName, routeValues)
                : controller.RedirectToAction(actionName);
        }
    }

}
