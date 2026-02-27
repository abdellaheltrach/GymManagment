using GymManagement.Application.Common.Behaviours;

namespace GymManagement.Application._Features.Progress.Commands.Models;

public record RecordProgressCommand(
    Guid TraineeId,
    string RecordedById,
    decimal? WeightKg,
    decimal? BodyFatPercent,
    decimal? MuscleMassKg,
    decimal? ChestCm,
    decimal? WaistCm,
    decimal? HipsCm,
    decimal? ArmCm,
    string? Notes
) : ICommand<Guid>;
