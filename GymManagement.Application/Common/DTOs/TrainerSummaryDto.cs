using GymManagement.Domain.Enums;

namespace GymManagement.Application.Common.DTOs;

// ── Trainer DTOs ──────────────────────────────────────────────────────────────

public record TrainerSummaryDto(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string Phone,
    TrainerSpecialization Specialization,
    int YearsOfExperience,
    int AssignedTraineeCount,
    bool IsActive
);
