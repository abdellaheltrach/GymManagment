using GymManagement.Domain.Enums;

namespace GymManagement.Application.Common.DTOs;

// ── Trainee DTOs ──────────────────────────────────────────────────────────────

public record TraineeSummaryDto(
    Guid Id,
    string FullName,
    string Email,
    string Phone,
    DateTime JoinDate,
    MembershipStatus? ActiveMembershipStatus,
    DateTime? MembershipEndDate
);
