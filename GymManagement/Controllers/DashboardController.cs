using GymManagement.Application._Features.Dashboard.Queries.Models;
using GymManagement.Web.Bases;
using GymManagement.Web.ViewModels.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Web.Controllers
{
    [AllowAnonymous]
    [Route("[controller]")]
    public class DashboardController : BaseController
    {
        [HttpGet]
        [Route("/")]

        public async Task<IActionResult> Index(CancellationToken ct)
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return RedirectToAction("Login", "Account", new { ReturnUrl = "/Dashboard" });
            }

            var result = await Mediator.Send(new GetAdminDashboardQuery(), ct);


            return HandleResult(result, data => View(new AdminDashboardViewModel
            {
                Data = data
            }));
        }
    }
}
