namespace GymManagement.Application.Common.DTOs;

// ── Progress DTOs ─────────────────────────────────────────────────────────────

public record ProgressRecordDto(
    Guid Id,
    DateTime RecordedAt,
    decimal? WeightKg,
    decimal? BodyFatPercent,
    decimal? MuscleMassKg,
    decimal? WaistCm,
    decimal? ChestCm,
    string? Notes
);
