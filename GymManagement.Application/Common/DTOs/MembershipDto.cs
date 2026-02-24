using GymManagement.Domain.Enums;

namespace GymManagement.Application.Common.DTOs;

// ── Membership DTOs ───────────────────────────────────────────────────────────

public record MembershipDto(
    Guid Id,
    Guid TraineeId,
    string PlanName,
    AccessLevel AccessLevel,
    DateTime StartDate,
    DateTime EndDate,
    MembershipStatus Status,
    decimal TotalAmount,
    decimal AmountPaid,
    decimal RemainingBalance,
    bool IsFullyPaid,
    int TotalFrozenDays,
    string? Notes
);
