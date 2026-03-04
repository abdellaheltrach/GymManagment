using GymManagement.Application.Features.Trainees.Queries.Models;
using GymManagement.Application.Features.Trainers.Queries.Models;
using GymManagement.Domain.Enums;
using GymManagement.Domain.Interfaces;
using GymManagement.Web.Bases;
using GymManagement.Web.ViewModels.Trainer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Web.Controllers
{
    [Authorize(Roles = "Trainer")]
    public class TrainerTraineesController : BaseController
    {
        private readonly IUnitOfWork _uow;

        public TrainerTraineesController(IUnitOfWork uow) => _uow = uow;

        // GET /TrainerTrainees
        [HttpGet]
        public async Task<IActionResult> Index(
            string? search = null,
            MembershipStatus? status = null,
            CancellationToken ct = default)
        {
            var trainerId = await ResolveTrainerIdAsync(ct);
            if (trainerId is null) return Forbid();

            var result = await Mediator.Send(
                new GetMyTraineesQuery(trainerId.Value, search, status), ct);

            return HandleResult(result, trainees => View(new MyTraineesListViewModel
            {
                Trainees = trainees,
                SearchTerm = search,
                StatusFilter = status
            }));
        }

        // GET /TrainerTrainees/Details?id={guid}
        [HttpGet]
        public async Task<IActionResult> Details(Guid id, CancellationToken ct)
        {
            var trainerId = await ResolveTrainerIdAsync(ct);
            if (trainerId is null) return Forbid();

            // Security: verify this trainee is actually assigned to this trainer
            var isAssigned = await _uow.TrainerAssignments.AnyAsync(
                a => a.TrainerId == trainerId.Value &&
                     a.TraineeId == id &&
                     a.RemovedAt == null, ct);

            if (!isAssigned) return Forbid();

            // Reuse existing admin query — same data, trainer just can't edit
            var result = await Mediator.Send(new GetTraineeByIdQuery(id), ct);

            // Load progress records for the chart
            var progressRecords = await _uow.ProgressRecords.FindAsync(
                p => p.TraineeId == id, ct);

            var sortedProgress = progressRecords
                .OrderBy(p => p.RecordedAt)
                .ToList();

            // Load attendance history
            var attendances = await _uow.Attendances.FindAsync(
                a => a.TraineeId == id, ct);

            var recentAttendance = attendances
                .OrderByDescending(a => a.CheckInTime)
                .Take(10)
                .ToList();

            return HandleResult(result, dto => View(new MyTraineeDetailViewModel
            {
                Trainee = dto,
                TrainerId = trainerId.Value,
                ProgressRecords = sortedProgress.Select(p => new ProgressPointViewModel(
                    p.RecordedAt.ToString("dd MMM"),
                    p.WeightKg,
                    p.BodyFatPercent,
                    p.MuscleMassKg,
                    p.WaistCm
                )).ToList(),
                RecentAttendance = recentAttendance.Select(a => new AttendanceRowViewModel(
                    a.CheckInTime,
                    a.CheckOutTime,
                    a.CheckOutTime.HasValue
                        ? (a.CheckOutTime.Value - a.CheckInTime).TotalMinutes
                        : null
                )).ToList()
            }));
        }

        // ── Helper: resolve domain TrainerId from Identity UserId ──────────────────
        private async Task<Guid?> ResolveTrainerIdAsync(CancellationToken ct)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return null;

            var trainers = await _uow.Trainers.FindAsync(
                t => t.ApplicationUserId == userId, ct);

            return trainers.FirstOrDefault()?.Id;
        }
    }

}
