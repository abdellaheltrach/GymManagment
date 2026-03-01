using GymManagement.Application.Features.Trainers.Commands.Models;
using GymManagement.Domain.Entities;
using GymManagement.Domain.Interfaces;
using GymManagement.Domain.Results;
using MediatR;

namespace GymManagement.Application.Features.Trainers.Commands.Handlers;

public class AssignTrainerCommandHandler(IUnitOfWork uow) : IRequestHandler<AssignTrainerCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(AssignTrainerCommand cmd, CancellationToken ct)
    {
        var trainer = await uow.Trainers.GetByIdAsync(cmd.TrainerId, ct);
        if (trainer is null || !trainer.IsActive)
            return Result<Guid>.NotFound("Trainer", cmd.TrainerId);

        var trainee = await uow.Trainees.GetByIdAsync(cmd.TraineeId, ct);
        if (trainee is null)
            return Result<Guid>.NotFound("Trainee", cmd.TraineeId);

        // Only one active trainer per trainee at a time
        var alreadyAssigned = await uow.TrainerAssignments.AnyAsync(
            a => a.TraineeId == cmd.TraineeId && a.RemovedAt == null, ct);

        if (alreadyAssigned)
            return Result<Guid>.Conflict(
                "Trainee already has an active trainer assignment. Remove it first.");

        var assignment = new TrainerAssignment
        {
            TrainerId = cmd.TrainerId,
            TraineeId = cmd.TraineeId,
            AssignedById = cmd.AssignedById,
            AssignedAt = DateTime.UtcNow
        };

        await uow.TrainerAssignments.AddAsync(assignment, ct);


        return Result<Guid>.Success(assignment.Id);
    }
}

