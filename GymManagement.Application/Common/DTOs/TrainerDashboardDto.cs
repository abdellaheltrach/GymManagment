namespace GymManagement.Application.Common.DTOs;

public record TrainerDashboardDto(
    int AssignedTrainees,
    int AttendanceMarkedThisMonth,
    int ActivePrograms,
    decimal? CommissionThisMonth,
    IReadOnlyList<TraineeSummaryDto> AssignedTraineeList
);
