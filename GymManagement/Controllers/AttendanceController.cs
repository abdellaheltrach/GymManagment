using GymManagement.Application._Features.Attendance.Commands.Models;
using GymManagement.Domain.Enums;
using GymManagement.Web.Bases;
using GymManagement.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Web.Controllers
{

    [Authorize(Policy = "CanMarkAttendance")]
    [Route("[controller]")]
    public class AttendanceController : BaseController
    {
        [HttpPost]
        public async Task<IActionResult> CheckIn(
            Guid traineeId, AttendanceMethod method, CancellationToken ct)
        {
            var result = await Mediator.Send(
                new CheckInCommand(traineeId, method, User.GetUserId()), ct);

            if (result.IsFailure)
                return RedirectWithError(result.Error!,
                    "Details", new { controller = "Trainees", id = traineeId });

            return RedirectWithSuccess("Check-in recorded.",
                "Details", new { controller = "Trainees", id = traineeId });
        }

        [HttpPost]
        public async Task<IActionResult> CheckOut(Guid traineeId, CancellationToken ct)
        {
            var result = await Mediator.Send(
                new CheckOutCommand(traineeId, User.GetUserId()), ct);

            if (result.IsFailure)
                return RedirectWithError(result.Error!,
                    "Details", new { controller = "Trainees", id = traineeId });

            return RedirectWithSuccess("Check-out recorded.",
                "Details", new { controller = "Trainees", id = traineeId });
        }
    }
}
