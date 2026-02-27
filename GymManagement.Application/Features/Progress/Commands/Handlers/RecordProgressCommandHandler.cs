using GymManagement.Application._Features.Progress.Commands.Models;
using GymManagement.Domain.Entities;
using GymManagement.Domain.Interfaces;
using GymManagement.Domain.Results;
using MediatR;

namespace GymManagement.Application._Features.Progress.Commands.Handlers;

public class RecordProgressCommandHandler(IUnitOfWork uow) : IRequestHandler<RecordProgressCommand, Result<Guid>>
{

    public async Task<Result<Guid>> Handle(RecordProgressCommand cmd, CancellationToken ct)
    {
        var trainee = await uow.Trainees.GetByIdAsync(cmd.TraineeId, ct);
        if (trainee is null)
            return Result<Guid>.NotFound("Trainee", cmd.TraineeId);

        var record = new ProgressRecord
        {
            TraineeId = cmd.TraineeId,
            RecordedById = cmd.RecordedById,
            RecordedAt = DateTime.UtcNow,
            WeightKg = cmd.WeightKg,
            BodyFatPercent = cmd.BodyFatPercent,
            MuscleMassKg = cmd.MuscleMassKg,
            ChestCm = cmd.ChestCm,
            WaistCm = cmd.WaistCm,
            HipsCm = cmd.HipsCm,
            ArmCm = cmd.ArmCm,
            Notes = cmd.Notes
        };

        await uow.ProgressRecords.AddAsync(record, ct);
        await uow.SaveChangesAsync(ct);

        return Result<Guid>.Success(record.Id);
    }
}
