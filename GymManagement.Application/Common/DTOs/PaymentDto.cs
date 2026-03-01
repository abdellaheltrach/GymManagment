using GymManagement.Domain.Enums;

namespace GymManagement.Application.Common.DTOs;

// ── Payment DTOs ──────────────────────────────────────────────────────────────

public record PaymentDto(
    Guid Id,
    Guid MembershipId,
    string TraineeName,
    string PlanName,
    decimal Amount,
    PaymentMethod Method,
    PaymentStatus Status,
    DateTime PaidAt,
    string? ReferenceNumber,
    bool IsRefunded,
    DateTime? RefundedAt,
    string? RefundReason
);
