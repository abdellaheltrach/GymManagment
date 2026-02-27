using GymManagement.Application._Features.Trainees.Commands.Models;
using GymManagement.Domain.Interfaces;
using GymManagement.Domain.Results;
using MediatR;

namespace GymManagement.Application._Features.Trainees.Commands.Handlers;

public class AnonymiseTraineeCommandHandler(IUnitOfWork uow) : IRequestHandler<AnonymiseTraineeCommand, Result>
{

    public async Task<Result> Handle(AnonymiseTraineeCommand cmd, CancellationToken ct)
    {
        var trainee = await uow.Trainees.GetByIdAsync(cmd.TraineeId, ct);
        if (trainee is null)
            return Result.NotFound("Trainee", cmd.TraineeId);

        if (trainee.IsAnonymised)
            return Result.Conflict("Trainee has already been anonymised.");

        var anonymisedId = $"ANON-{cmd.TraineeId:N}";

        trainee.FirstName = "Anonymised";
        trainee.LastName = "User";
        trainee.Email = $"{anonymisedId}@deleted.local";
        trainee.Phone = "000-000-0000";
        trainee.NationalId = anonymisedId;
        trainee.Address = null;
        trainee.MedicalNotes = null;
        trainee.PhotoPath = null;
        trainee.EmergencyContactName = "Anonymised";
        trainee.EmergencyContactPhone = "000-000-0000";
        trainee.EmergencyContactRelation = null;
        trainee.ApplicationUserId = null;
        trainee.IsAnonymised = true;
        trainee.UpdatedById = cmd.RequestedById;

        uow.Trainees.Update(trainee);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
