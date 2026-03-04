using GymManagement.Domain.Enums;

namespace GymManagement.Application.Common.DTOs;

public record MembershipPlanDto(
    Guid Id,
    string Name,
    string? Description,
    int DurationDays,
    decimal Price,
    AccessLevel AccessLevel,
    bool IncludesPersonalTrainer,
    decimal TrainerAddonFee,
    int MaxFreezeDays,
    bool IsActive
);
