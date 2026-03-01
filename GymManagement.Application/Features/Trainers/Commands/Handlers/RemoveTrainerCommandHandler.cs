using GymManagement.Application.Features.Trainers.Commands.Models;
using GymManagement.Domain.Interfaces;
using GymManagement.Domain.Results;
using MediatR;

namespace GymManagement.Application.Features.Trainers.Commands.Handlers;

public class RemoveTrainerCommandHandler(IUnitOfWork uow) : IRequestHandler<RemoveTrainerCommand, Result>
{
    public async Task<Result> Handle(RemoveTrainerCommand cmd, CancellationToken ct)
    {
        var assignment = await uow.TrainerAssignments.FirstOrDefaultAsync(
            a => a.TraineeId == cmd.TraineeId && a.RemovedAt == null, ct);

        if (assignment is null)
            return Result.NotFound("No active trainer assignment found for this trainee.");

        // Soft-remove — history preserved, never deleted
        assignment.RemovedAt = DateTime.UtcNow;
        assignment.RemovedById = cmd.RemovedById;
        assignment.RemovalReason = cmd.Reason;

        uow.TrainerAssignments.Update(assignment);


        return Result.Success();
    }
}

