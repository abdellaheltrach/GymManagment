using FluentValidation;
using GymManagement.Application.Common.Behaviours;
using GymManagement.Domain.Entities;
using GymManagement.Domain.Enums;
using GymManagement.Domain.Interfaces;
using GymManagement.Domain.Results;
using MediatR;

namespace GymManagement.Application.Features.Trainers.Commands.AssignOrReassignTrainer;

/// <summary>
/// Assigns a trainer to a trainee, or replaces the current trainer.
///
/// BUSINESS RULES:
///   1. Trainee must have an Active membership.
///   2. That membership's plan must have IncludesPersonalTrainer = true.
///   3. The target trainer must be active.
///   4. If the trainee already has a trainer:
///      - Soft-remove the current assignment (history preserved).
///      - Create a new assignment for the new trainer.
///   5. If no current trainer: just create the assignment.
///
/// WHY a new command instead of reusing AssignTrainerCommand:
///   AssignTrainerCommand does not check subscriptions and errors if a trainer
///   is already assigned. This command handles both cases atomically and adds
///   the subscription gate.
/// </summary>
public record AssignOrReassignTrainerCommand(
    Guid TraineeId,
    Guid NewTrainerId,
    string AssignedById
) : ICommand<Guid>;

public class AssignOrReassignTrainerHandler
    : IRequestHandler<AssignOrReassignTrainerCommand, Result<Guid>>
{
    private readonly IUnitOfWork _uow;

    public AssignOrReassignTrainerHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<Guid>> Handle(
        AssignOrReassignTrainerCommand cmd, CancellationToken ct)
    {
        // ── 1. Load trainee ────────────────────────────────────────────────────
        var trainee = await _uow.Trainees.GetByIdAsync(cmd.TraineeId, ct);
        if (trainee is null)
            return Result<Guid>.NotFound("Trainee", cmd.TraineeId);

        // ── 2. Check active membership exists ──────────────────────────────────
        var activeMemberships = await _uow.Memberships.FindAsync(
            m => m.TraineeId == cmd.TraineeId &&
                 m.Status == MembershipStatus.Active &&
                 m.EndDate > DateTime.UtcNow, ct);

        var membership = activeMemberships.FirstOrDefault();

        if (membership is null)
            return Result<Guid>.Failure(
                "Trainee does not have an active membership. " +
                "A trainer can only be assigned to members with an active subscription.");

        // ── 3. Check plan supports trainer AND add-on fee is paid ─────────────
        // Rule: plan must have IncludesPersonalTrainer = true AND TrainerAddonPaid
        // must be true on the current membership cycle.
        // If TrainerAddonFee == 0 the admin still runs PayTrainerAddonCommand
        // (zero-amount) to mark it paid — that remains the single gate.
        var plan = await _uow.MembershipPlans.GetByIdAsync(membership.PlanId, ct);

        if (plan is null || !plan.IncludesPersonalTrainer)
            return Result<Guid>.Failure(
                $"The trainee's current plan '{plan?.Name ?? "Unknown"}' does not include " +
                "personal trainer access. Please upgrade their membership plan first.");

        if (!membership.TrainerAddonPaid)
            return Result<Guid>.Failure(
                "The personal trainer add-on fee has not been paid for this membership cycle. " +
                $"The add-on fee is {plan.TrainerAddonFee:C}. " +
                "Please record the add-on payment before assigning a trainer.");

        // ── 4. Load target trainer ─────────────────────────────────────────────
        var newTrainer = await _uow.Trainers.GetByIdAsync(cmd.NewTrainerId, ct);

        if (newTrainer is null || !newTrainer.IsActive)
            return Result<Guid>.NotFound("Trainer", cmd.NewTrainerId);

        // ── 5. Handle existing assignment ──────────────────────────────────────
        var currentAssignment = await _uow.TrainerAssignments.FirstOrDefaultAsync(
            a => a.TraineeId == cmd.TraineeId && a.RemovedAt == null, ct);

        // If same trainer is already assigned — nothing to do
        if (currentAssignment is not null &&
            currentAssignment.TrainerId == cmd.NewTrainerId)
            return Result<Guid>.Conflict(
                $"{newTrainer.FullName} is already assigned to this trainee.");

        // Soft-remove the current assignment if one exists
        if (currentAssignment is not null)
        {
            currentAssignment.RemovedAt = DateTime.UtcNow;
            currentAssignment.RemovedById = cmd.AssignedById;
            currentAssignment.RemovalReason = "Reassigned to another trainer";
            _uow.TrainerAssignments.Update(currentAssignment);
        }

        // ── 6. Create new assignment ───────────────────────────────────────────
        var newAssignment = new TrainerAssignment
        {
            TrainerId = cmd.NewTrainerId,
            TraineeId = cmd.TraineeId,
            AssignedById = cmd.AssignedById,
            AssignedAt = DateTime.UtcNow
        };

        await _uow.TrainerAssignments.AddAsync(newAssignment, ct);
        await _uow.SaveChangesAsync(ct);

        return Result<Guid>.Success(newAssignment.Id);
    }
}

public class AssignOrReassignTrainerValidator
    : AbstractValidator<AssignOrReassignTrainerCommand>
{
    public AssignOrReassignTrainerValidator()
    {
        RuleFor(x => x.TraineeId).NotEmpty();
        RuleFor(x => x.NewTrainerId).NotEmpty();
        RuleFor(x => x.AssignedById).NotEmpty();
    }
}
