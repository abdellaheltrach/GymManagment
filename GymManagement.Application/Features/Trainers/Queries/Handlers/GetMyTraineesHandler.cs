using GymManagement.Application.Common.DTOs;
using GymManagement.Application.Features.Trainers.Queries.Models;
using GymManagement.Domain.Enums;
using GymManagement.Domain.Interfaces;
using GymManagement.Domain.Results;
using MediatR;

namespace GymManagement.Application.Features.Trainers.Queries.Handlers
{
    public class GetMyTraineesHandler
        : IRequestHandler<GetMyTraineesQuery, Result<IReadOnlyList<MyTraineeSummaryDto>>>
    {
        private readonly IUnitOfWork _uow;

        public GetMyTraineesHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<Result<IReadOnlyList<MyTraineeSummaryDto>>> Handle(
            GetMyTraineesQuery query, CancellationToken ct)
        {
            var now = DateTime.UtcNow;
            var today = now.Date;

            // ── Get all active assignments for this trainer ─────────────────────────
            var assignments = await _uow.TrainerAssignments.FindAsync(
                a => a.TrainerId == query.TrainerId && a.RemovedAt == null, ct);

            var traineeIds = assignments.Select(a => a.TraineeId).ToList();

            if (traineeIds.Count == 0)
                return Result<IReadOnlyList<MyTraineeSummaryDto>>.Success(
                    Array.Empty<MyTraineeSummaryDto>());

            // ── Batch load all needed data ─────────────────────────────────────────
            var memberships = await _uow.Memberships.FindAsync(
                m => traineeIds.Contains(m.TraineeId) &&
                     m.Status == MembershipStatus.Active, ct);

            var membershipMap = memberships.ToDictionary(m => m.TraineeId);

            var todayAttendances = await _uow.Attendances.FindAsync(
                a => traineeIds.Contains(a.TraineeId) && a.CheckInTime >= today, ct);

            var checkedInTodaySet = todayAttendances
                .Select(a => a.TraineeId)
                .ToHashSet();

            var allAttendances = await _uow.Attendances.FindAsync(
                a => traineeIds.Contains(a.TraineeId), ct);

            var attendanceByTrainee = allAttendances
                .GroupBy(a => a.TraineeId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(a => a.CheckInTime).ToList());

            var progressRecords = await _uow.ProgressRecords.FindAsync(
                p => traineeIds.Contains(p.TraineeId), ct);

            var lastProgressMap = progressRecords
                .GroupBy(p => p.TraineeId)
                .ToDictionary(g => g.Key, g => g.Max(p => p.RecordedAt));

            // ── Build DTOs ─────────────────────────────────────────────────────────
            var results = new List<MyTraineeSummaryDto>();

            foreach (var traineeId in traineeIds)
            {
                var trainee = await _uow.Trainees.GetByIdAsync(traineeId, ct);
                if (trainee is null) continue;

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(query.SearchTerm))
                {
                    var term = query.SearchTerm.ToLower();
                    if (!trainee.FullName.ToLower().Contains(term) &&
                        !trainee.Phone.Contains(term) &&
                        !trainee.Email.ToLower().Contains(term))
                        continue;
                }

                membershipMap.TryGetValue(traineeId, out var membership);

                // Apply status filter
                if (query.StatusFilter.HasValue && membership?.Status != query.StatusFilter)
                    continue;

                var daysUntilExpiry = membership is not null
                    ? Math.Max(0, (int)(membership.EndDate - now).TotalDays)
                    : 0;

                attendanceByTrainee.TryGetValue(traineeId, out var attendances);
                lastProgressMap.TryGetValue(traineeId, out var lastProgress);

                results.Add(new MyTraineeSummaryDto(
                    TraineeId: traineeId,
                    FullName: trainee.FullName,
                    Email: trainee.Email,
                    Phone: trainee.Phone,
                    Gender: trainee.Gender,
                    JoinDate: trainee.JoinDate,
                    MembershipStatus: membership?.Status,
                    MembershipEndDate: membership?.EndDate,
                    DaysUntilExpiry: daysUntilExpiry,
                    CheckedInToday: checkedInTodaySet.Contains(traineeId),
                    LastCheckIn: attendances?.FirstOrDefault()?.CheckInTime,
                    TotalSessions: attendances?.Count ?? 0,
                    LastProgressDate: lastProgress == default ? null : lastProgress
                ));
            }

            var sorted = results
                .OrderBy(t => t.FullName)
                .ToList()
                .AsReadOnly();

            return Result<IReadOnlyList<MyTraineeSummaryDto>>.Success(sorted);
        }
    }

}
