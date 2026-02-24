namespace GymManagement.Application.Common.DTOs;


public record AdminDashboardDto(
    int TotalActiveMembers,
    int NewMembersThisMonth,
    decimal MonthlyRevenue,
    int ExpiringThisWeek,
    int PendingPayments,
    IReadOnlyList<TrainerSummaryDto> TopTrainers
);
