using GymManagement.Application.Common.Behaviours;
using GymManagement.Application.Common.DTOs;
using GymManagement.Domain.Interfaces;
using GymManagement.Domain.Results;
using MediatR;

namespace GymManagement.Application.Features.Progress.Queries.GetTraineeProgress;

public record GetTraineeProgressQuery(Guid TraineeId)
    : IQuery<TraineeProgressDto>;

/// <summary>
/// Full progress history + computed delta between first and latest record.
/// Delta lets the trainer show the trainee total improvement over time.
/// </summary>
public record TraineeProgressDto(
    string TraineeName,
    IReadOnlyList<ProgressRecordDto> Records,
    decimal? WeightDeltaKg,       // negative = weight lost
    decimal? BodyFatDeltaPercent,  // negative = fat lost
    decimal? MuscleDeltaKg,        // positive = muscle gained
    decimal? WaistDeltaCm
);

public class GetTraineeProgressHandler
    : IRequestHandler<GetTraineeProgressQuery, Result<TraineeProgressDto>>
{
    private readonly IUnitOfWork _uow;
    public GetTraineeProgressHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<TraineeProgressDto>> Handle(
        GetTraineeProgressQuery query, CancellationToken ct)
    {
        var trainee = await _uow.Trainees.GetByIdAsync(query.TraineeId, ct);
        if (trainee is null)
            return Result<TraineeProgressDto>.NotFound("Trainee", query.TraineeId);

        var records = await _uow.ProgressRecords.FindAsync(
            p => p.TraineeId == query.TraineeId, ct);

        var sorted = records
            .OrderBy(p => p.RecordedAt)
            .ToList();

        var dtos = sorted.Select(p => new ProgressRecordDto(
            p.Id, p.RecordedAt, p.WeightKg, p.BodyFatPercent,
            p.MuscleMassKg, p.WaistCm, p.ChestCm, p.Notes
        )).ToList().AsReadOnly();

        // Compute deltas only if we have at least 2 records
        decimal? weightDelta = null, fatDelta = null,
                 muscleDelta = null, waistDelta = null;

        if (sorted.Count >= 2)
        {
            var first = sorted.First();
            var latest = sorted.Last();

            if (first.WeightKg.HasValue && latest.WeightKg.HasValue)
                weightDelta = latest.WeightKg - first.WeightKg;

            if (first.BodyFatPercent.HasValue && latest.BodyFatPercent.HasValue)
                fatDelta = latest.BodyFatPercent - first.BodyFatPercent;

            if (first.MuscleMassKg.HasValue && latest.MuscleMassKg.HasValue)
                muscleDelta = latest.MuscleMassKg - first.MuscleMassKg;

            if (first.WaistCm.HasValue && latest.WaistCm.HasValue)
                waistDelta = latest.WaistCm - first.WaistCm;
        }

        return Result<TraineeProgressDto>.Success(new TraineeProgressDto(
            trainee.FullName, dtos,
            weightDelta, fatDelta, muscleDelta, waistDelta));
    }
}
