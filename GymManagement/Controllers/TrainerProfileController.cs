using GymManagement.Application.Features.Trainers.Queries.GetMyProfile;
using GymManagement.Web.Bases;
using GymManagement.Web.ViewModels.TrainerProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GymManagement.Web.Controllers;

[Authorize(Roles = "Trainer")]
public class TrainerProfileController : BaseController
{
    // GET /TrainerProfile
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Forbid();

        var result = await Mediator.Send(
            new GetMyProfileQuery(userId), ct);

        return HandleResult(result, dto => View(
            new TrainerProfileViewModel { Profile = dto }));
    }
}
