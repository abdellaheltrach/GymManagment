using GymManagement.Application.Common.Models;
using GymManagement.Domain.Interfaces;
using MediatR;
using GymManagement.Application._Features.Trainees.Commands.Models;

namespace GymManagement.Application._Features.Trainees.Commands.Handlers;

public class DeleteTraineeCommandHandler(IUnitOfWork uow) : IRequestHandler<DeleteTraineeCommand, Result>
{

    public async Task<Result> Handle(DeleteTraineeCommand cmd, CancellationToken ct)
    {
        var trainee = await uow.Trainees.GetByIdAsync(cmd.TraineeId, ct);
        if (trainee is null)
            return Result.NotFound("Trainee", cmd.TraineeId);

        // Cannot delete a trainee with an active membership
        var hasActiveMembership = await uow.Memberships.AnyAsync(
            m => m.TraineeId == cmd.TraineeId &&
                 m.Status == Domain.Enums.MembershipStatus.Active, ct);

        if (hasActiveMembership)
            return Result.Conflict("Cannot delete a trainee with an active membership. Suspend or expire the membership first.");

        trainee.DeletedById = cmd.DeletedById;
        uow.Trainees.Remove(trainee); // AppDbContext converts this to soft delete
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
