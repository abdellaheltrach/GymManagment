using GymManagement.Application.Common.DTOs;
using GymManagement.Domain.Enums;

namespace GymManagement.Web.ViewModels.Trainer
{
    public class MyTraineesListViewModel
    {
        public IReadOnlyList<MyTraineeSummaryDto> Trainees { get; init; } = [];
        public string? SearchTerm { get; init; }
        public MembershipStatus? StatusFilter { get; init; }

        public int TotalCount => Trainees.Count;
        public int CheckedInToday => Trainees.Count(t => t.CheckedInToday);
        public int ActiveCount => Trainees.Count(t => t.MembershipStatus == MembershipStatus.Active);
        public int ExpiringSoon => Trainees.Count(t => t.DaysUntilExpiry is > 0 and <= 7);
    }

    public class MyTraineeDetailViewModel
    {
        public required TraineeDetailDto Trainee { get; init; }
        public required Guid TrainerId { get; init; }
        public required List<ProgressPointViewModel> ProgressRecords { get; init; }
        public required List<AttendanceRowViewModel> RecentAttendance { get; init; }

        public bool HasProgress => ProgressRecords.Count > 0;
        public bool HasAttendance => RecentAttendance.Count > 0;

        public string MembershipStatusBadge => Trainee.ActiveMembership?.Status switch
        {
            MembershipStatus.Active => "badge bg-success",
            MembershipStatus.Frozen => "badge bg-info text-dark",
            MembershipStatus.Expired => "badge bg-danger",
            MembershipStatus.PendingPayment => "badge bg-warning text-dark",
            MembershipStatus.Suspended => "badge bg-secondary",
            _ => "badge bg-light text-dark"
        };
    }

    public record ProgressPointViewModel(
        string Label,
        decimal? WeightKg,
        decimal? BodyFatPercent,
        decimal? MuscleMassKg,
        decimal? WaistCm
    );

    public record AttendanceRowViewModel(
        DateTime CheckIn,
        DateTime? CheckOut,
        double? DurationMinutes
    );

}
