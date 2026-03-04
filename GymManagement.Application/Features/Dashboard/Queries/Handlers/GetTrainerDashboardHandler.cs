using GymManagement.Application.Common.DTOs;
using GymManagement.Application.Features.Dashboard.Queries.Models;
using GymManagement.Domain.Enums;
using GymManagement.Domain.Interfaces;
using GymManagement.Domain.Results;
using MediatR;

namespace GymManagement.Application.Features.Dashboard.Queries.Handlers
{
    public class GetTrainerDashboardHandler
        : IRequestHandler<GetTrainerDashboardQuery, Result<TrainerDashboardDto>>
    {
        private readonly IUnitOfWork _uow;

        public GetTrainerDashboardHandler(IUnitOfWork uow) => _uow = uow;

        #region handler
        public async Task<Result<TrainerDashboardDto>> Handle(
            GetTrainerDashboardQuery query,
            CancellationToken ct)
        {
            var trainerId = query.TrainerId;
            var now = DateTime.UtcNow;
            var today = now.Date;
            var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            // ── Active assignments for this trainer ────────────────────────────────
            var assignments = await _uow.TrainerAssignments.FindAsync(
                a => a.TrainerId == trainerId && a.RemovedAt == null, ct);

            var traineeIds = assignments.Select(a => a.TraineeId).ToList();

            // ── Trainee summaries ──────────────────────────────────────────────────
            var traineeSummaries = new List<TraineeDashboardSummary>();

            foreach (var traineeId in traineeIds)
            {
                var trainee = await _uow.Trainees.GetByIdAsync(traineeId, ct);
                if (trainee is null) continue;

                var activeMembership = await _uow.Memberships.FindAsync(
                    m => m.TraineeId == traineeId &&
                         m.Status == MembershipStatus.Active &&
                         m.EndDate > now, ct);

                // Checked in today?
                var checkedInToday = await _uow.Attendances.AnyAsync(
                    a => a.TraineeId == traineeId &&
                         a.CheckInTime >= today, ct);

                // Has open check-in (checked in but not out)?
                var hasOpenCheckIn = await _uow.Attendances.AnyAsync(
                    a => a.TraineeId == traineeId &&
                         a.CheckInTime >= today &&
                         a.CheckOutTime == null, ct);

                traineeSummaries.Add(new TraineeDashboardSummary(
                    traineeId,
                    trainee.FullName,
                    trainee.Phone,
                    activeMembership.FirstOrDefault()?.Status,
                    activeMembership.FirstOrDefault()?.EndDate,
                    checkedInToday,
                    hasOpenCheckIn
                ));
            }

            // ── Attendance recorded by this trainer this month ─────────────────────
            var attendanceThisMonth = await _uow.Attendances.CountAsync(
                a => traineeIds.Contains(a.TraineeId) &&
                     a.CheckInTime >= monthStart, ct);

            // ── Active training programs created by this trainer ───────────────────
            var activePrograms = await _uow.TrainingPrograms.CountAsync(
                p => p.TrainerId == trainerId && p.IsActive, ct);

            // ── Commission this month (only for commission-based trainers) ─────────
            var trainer = await _uow.Trainers.GetByIdAsync(trainerId, ct);
            decimal? commissionThisMonth = null;

            if (trainer is not null && trainer.SalaryType == SalaryType.Commission
                && trainer.CommissionPerTrainee.HasValue)
            {
                commissionThisMonth = traineeIds.Count * trainer.CommissionPerTrainee.Value;
            }

            // ── Trainees checked in today ──────────────────────────────────────────
            var checkedInTodayCount = traineeSummaries.Count(t => t.CheckedInToday);

            var dto = new TrainerDashboardDto(
                AssignedTrainees: traineeIds.Count,
                AttendanceMarkedThisMonth: attendanceThisMonth,
                ActivePrograms: activePrograms,
                CommissionThisMonth: commissionThisMonth,
                AssignedTraineeList: traineeSummaries
                    .Select(t => new TraineeSummaryDto(
                        t.TraineeId,
                        t.FullName,
                        string.Empty,   // email not shown on trainer dashboard
                        t.Phone,
                        DateTime.MinValue,
                        t.MembershipStatus,
                        t.MembershipEndDate))
                    .ToList()
                    .AsReadOnly()
            );

            return Result<TrainerDashboardDto>.Success(dto);
        }
        #endregion



        internal record TraineeDashboardSummary(
    Guid TraineeId,
    string FullName,
    string Phone,
    MembershipStatus? MembershipStatus,
    DateTime? MembershipEndDate,
    bool CheckedInToday,
    bool HasOpenCheckIn);

    }
}
