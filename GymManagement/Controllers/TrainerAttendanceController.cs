
using GymManagement.Application.Features.Attendance.Commands.Models;
using GymManagement.Domain.Enums;
using GymManagement.Domain.Interfaces;
using GymManagement.Web.Bases;
using GymManagement.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Web.Controllers;

/// <summary>
/// Trainer-scoped attendance controller.
/// Reuses existing CheckInCommand and CheckOutCommand from the Application layer.
/// Security: verifies the trainee is assigned to this trainer before acting.
/// Redirects back to TrainerTrainees views, not admin Trainees views.
/// </summary>
[Authorize(Roles = "Trainer")]
public class TrainerAttendanceController : BaseController
{
    private readonly IUnitOfWork _uow;
    public TrainerAttendanceController(IUnitOfWork uow) => _uow = uow;

    // POST /TrainerAttendance/CheckIn
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckIn(
        Guid traineeId,
        AttendanceMethod method = AttendanceMethod.Manual,
        CancellationToken ct = default)
    {
        var trainerId = await ResolveTrainerIdAsync(ct);
        if (trainerId is null) return Forbid();

        // Security: trainee must be assigned to this trainer
        var isAssigned = await _uow.TrainerAssignments.AnyAsync(
            a => a.TrainerId == trainerId.Value &&
                 a.TraineeId == traineeId &&
                 a.RemovedAt == null, ct);

        if (!isAssigned)
            return RedirectWithError(
                "This trainee is not assigned to you.",
                "Index",
                new { controller = "TrainerTrainees" });

        var result = await Mediator.Send(
            new CheckInCommand(traineeId, method, User.GetUserId()), ct);

        if (result.IsFailure)
            return RedirectWithError(result.Error!,
                "Details", new { controller = "TrainerTrainees", id = traineeId });

        return RedirectWithSuccess("Check-in recorded.",
            "Details", new { controller = "TrainerTrainees", id = traineeId });
    }

    // POST /TrainerAttendance/CheckOut
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckOut(
        Guid traineeId,
        CancellationToken ct = default)
    {
        var trainerId = await ResolveTrainerIdAsync(ct);
        if (trainerId is null) return Forbid();

        var isAssigned = await _uow.TrainerAssignments.AnyAsync(
            a => a.TrainerId == trainerId.Value &&
                 a.TraineeId == traineeId &&
                 a.RemovedAt == null, ct);

        if (!isAssigned)
            return RedirectWithError(
                "This trainee is not assigned to you.",
                "Index",
                new { controller = "TrainerTrainees" });

        var result = await Mediator.Send(
            new CheckOutCommand(traineeId, User.GetUserId()), ct);

        if (result.IsFailure)
            return RedirectWithError(result.Error!,
                "Details", new { controller = "TrainerTrainees", id = traineeId });

        return RedirectWithSuccess("Check-out recorded.",
            "Details", new { controller = "TrainerTrainees", id = traineeId });
    }

    private async Task<Guid?> ResolveTrainerIdAsync(CancellationToken ct)
    {
        var userId = User.FindFirst(
            System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return null;
        var trainers = await _uow.Trainers.FindAsync(
            t => t.ApplicationUserId == userId, ct);
        return trainers.FirstOrDefault()?.Id;
    }
}
