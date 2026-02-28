using GymManagement.Web.ViewModels.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Web.Controllers
{
    public class HomeController : Controller
    {
        [AllowAnonymous]

        public IActionResult Error(string? id)
        {
            if (id == "404" || id == "NotFound") return View("NotFound");
            if (id == "409" || id == "Conflict") return View("Conflict");
            if (id == "BadRequest" || id == "ValidationError") return View("ValidationError");
            if (id == "403" || id == "AccessDenied") return View("AccessDenied");

            return View(new ErrorViewModel { ErrorMessage = id ?? "An unexpected error occurred." });
        }
    }
}
