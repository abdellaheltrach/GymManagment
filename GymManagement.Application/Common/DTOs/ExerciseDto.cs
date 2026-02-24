namespace GymManagement.Application.Common.DTOs;

public record ExerciseDto(
    Guid Id,
    string Name,
    int Sets,
    int Reps,
    decimal? WeightKg,
    int? DurationSeconds,
    int? RestSeconds,
    int Order,
    string? Notes
);
