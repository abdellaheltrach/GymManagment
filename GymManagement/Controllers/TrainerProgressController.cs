using GymManagement.Application.Features.Progress.Commands.Models;
using GymManagement.Application.Features.Progress.Queries.GetTraineeProgress;
using GymManagement.Domain.Interfaces;
using GymManagement.Web.Bases;
using GymManagement.Web.Extensions;
using GymManagement.Web.ViewModels.TrainerProgress;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Web.Controllers;

[Authorize(Roles = "Trainer")]
public class TrainerProgressController : BaseController
{
    private readonly IUnitOfWork _uow;
    public TrainerProgressController(IUnitOfWork uow) => _uow = uow;

    // GET /TrainerProgress
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var trainerId = await ResolveTrainerIdAsync(ct);
        if (trainerId is null) return Forbid();

        var assignments = await _uow.TrainerAssignments.FindAsync(
            a => a.TrainerId == trainerId.Value && a.RemovedAt == null, ct);

        var vm = new TrainerProgressIndexViewModel();
        foreach (var a in assignments)
        {
            var trainee = await _uow.Trainees.GetByIdAsync(a.TraineeId, ct);
            if (trainee is not null)
            {
                var latest = (await _uow.ProgressRecords.FindAsync(
                    p => p.TraineeId == a.TraineeId, ct))
                    .OrderByDescending(p => p.RecordedAt)
                    .FirstOrDefault();

                vm.Trainees.Add(new TraineeProgressSummaryViewModel
                {
                    TraineeId = trainee.Id,
                    FullName = trainee.FullName,
                    LatestWeight = latest?.WeightKg,
                    LatestRecordedAt = latest?.RecordedAt
                });
            }
        }

        return View(vm);
    }

    // GET /TrainerProgress/Record?traineeId={guid}
    [HttpGet]
    public async Task<IActionResult> Record(
        Guid traineeId, CancellationToken ct)
    {
        var trainerId = await ResolveTrainerIdAsync(ct);
        if (trainerId is null) return Forbid();

        if (!await IsAssignedAsync(trainerId.Value, traineeId, ct))
            return Forbid();

        var trainee = await _uow.Trainees.GetByIdAsync(traineeId, ct);
        if (trainee is null) return NotFound();

        return View(new RecordProgressViewModel
        {
            TraineeId = traineeId,
            TraineeName = trainee.FullName
        });
    }

    // POST /TrainerProgress/Record
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Record(
        RecordProgressViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(vm);

        var trainerId = await ResolveTrainerIdAsync(ct);
        if (trainerId is null) return Forbid();

        if (!await IsAssignedAsync(trainerId.Value, vm.TraineeId, ct))
            return Forbid();

        var result = await Mediator.Send(new RecordProgressCommand(
            vm.TraineeId, User.GetUserId(),
            vm.WeightKg, vm.BodyFatPercent, vm.MuscleMassKg,
            vm.ChestCm, vm.WaistCm, vm.HipsCm, vm.ArmCm,
            vm.Notes), ct);

        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(vm);
        }

        return RedirectWithSuccess(
            "Progress recorded successfully.",
            "History",
            new { traineeId = vm.TraineeId });
    }

    // GET /TrainerProgress/History?traineeId={guid}
    [HttpGet]
    public async Task<IActionResult> History(
        Guid traineeId, CancellationToken ct)
    {
        var trainerId = await ResolveTrainerIdAsync(ct);
        if (trainerId is null) return Forbid();

        if (!await IsAssignedAsync(trainerId.Value, traineeId, ct))
            return Forbid();

        var result = await Mediator.Send(
            new GetTraineeProgressQuery(traineeId), ct);

        return HandleResult(result, dto => View(
            new ProgressHistoryViewModel
            {
                TraineeId = traineeId,
                TraineeName = dto.TraineeName,
                Records = dto.Records,
                WeightDelta = dto.WeightDeltaKg,
                BodyFatDelta = dto.BodyFatDeltaPercent,
                MuscleDelta = dto.MuscleDeltaKg,
                WaistDelta = dto.WaistDeltaCm
            }));
    }

    private async Task<bool> IsAssignedAsync(
        Guid trainerId, Guid traineeId, CancellationToken ct)
        => await _uow.TrainerAssignments.AnyAsync(
            a => a.TrainerId == trainerId &&
                 a.TraineeId == traineeId &&
                 a.RemovedAt == null, ct);

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
