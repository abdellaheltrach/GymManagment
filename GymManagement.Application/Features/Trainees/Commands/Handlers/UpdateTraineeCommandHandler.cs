using GymManagement.Application.Features.Trainees.Commands.Models;
using GymManagement.Domain.Interfaces;
using GymManagement.Domain.Results;
using MediatR;

namespace GymManagement.Application.Features.Trainees.Commands.Handlers;

public class UpdateTraineeCommandHandler(IUnitOfWork uow) : IRequestHandler<UpdateTraineeCommand, Result<Guid>>
{

    public async Task<Result<Guid>> Handle(UpdateTraineeCommand cmd, CancellationToken ct)
    {
        var trainee = await uow.Trainees.GetByIdAsync(cmd.TraineeId, ct);
        if (trainee is null)
            return Result<Guid>.NotFound("Trainee", cmd.TraineeId);

        trainee.FirstName = cmd.FirstName;
        trainee.LastName = cmd.LastName;
        trainee.Phone = cmd.Phone;
        trainee.Address = cmd.Address;
        trainee.MedicalNotes = cmd.MedicalNotes;
        trainee.HeightCm = cmd.HeightCm;
        trainee.WeightKg = cmd.WeightKg;
        trainee.EmergencyContactName = cmd.EmergencyContactName;
        trainee.EmergencyContactPhone = cmd.EmergencyContactPhone;
        trainee.EmergencyContactRelation = cmd.EmergencyContactRelation;
        trainee.UpdatedById = cmd.UpdatedById;

        uow.Trainees.Update(trainee);
        await uow.SaveChangesAsync(ct);

        return Result<Guid>.Success(trainee.Id);
    }
}
