using GymManagement.Application._Features.Dashboard.Queries.Models;
using GymManagement.Web.Bases;
using GymManagement.Web.ViewModels.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Web.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class DashboardController : BaseController
    {
        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var result = await Mediator.Send(new GetAdminDashboardQuery(), ct);

            return HandleResult(result, data => View(new AdminDashboardViewModel
            {
                Data = data
            }));
        }
    }
}
