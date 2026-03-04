using GymManagement.Domain.Enums;

namespace GymManagement.Application.Common.DTOs
{
    public record MyTraineeSummaryDto(
        Guid TraineeId,
        string FullName,
        string Email,
        string Phone,
        Gender Gender,
        DateTime JoinDate,
        MembershipStatus? MembershipStatus,
        DateTime? MembershipEndDate,
        int DaysUntilExpiry,
        bool CheckedInToday,
        DateTime? LastCheckIn,
        int TotalSessions,
        DateTime? LastProgressDate
    );

}
