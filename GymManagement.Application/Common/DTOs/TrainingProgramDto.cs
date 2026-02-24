namespace GymManagement.Application.Common.DTOs;

// ── Training Program DTOs ─────────────────────────────────────────────────────

public record TrainingProgramDto(
    Guid Id,
    Guid TrainerId,
    string TrainerName,
    Guid TraineeId,
    string TraineeName,
    string Title,
    string? Description,
    DateTime StartDate,
    DateTime? EndDate,
    bool IsActive,
    IReadOnlyList<ExerciseDto> Exercises
);
