using GymManagement.Domain.Enums;

namespace GymManagement.Application.Common.DTOs;

// ── Trainer DTOs ──────────────────────────────────────────────────────────────

public record TrainerSummaryDto(
    Guid Id,
    string FullName,
    string Email,
    TrainerSpecialization Specialization,
    int AssignedTraineeCount,
    bool IsActive
);
