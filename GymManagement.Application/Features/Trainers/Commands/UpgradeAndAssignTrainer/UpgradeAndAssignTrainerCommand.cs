using FluentValidation;
using GymManagement.Application.Common.Behaviours;
using GymManagement.Domain.Entities;
using GymManagement.Domain.Enums;
using GymManagement.Domain.Interfaces;
using GymManagement.Domain.Results;
using MediatR;

namespace GymManagement.Application.Features.Trainers.Commands.UpgradeAndAssignTrainer;

/// <summary>
/// A bundle command that:
///   1. Upgrades the trainee to a new membership plan that includes personal trainers.
///   2. Records the full payment for the new plan + any trainer add-on fee.
///   3. Assigns the selected coach.
///
/// This is used when a trainee is on a plan without trainer access but wants
/// to "Buy Access & Assign Coach" in one go.
/// </summary>
public record UpgradeAndAssignTrainerCommand(
    Guid          TraineeId,
    Guid          NewPlanId,
    Guid          TrainerId,
    PaymentMethod Method,
    string        AdminId
) : ICommand<Guid>; // returns the TrainerAssignment.Id

public class UpgradeAndAssignTrainerHandler
    : IRequestHandler<UpgradeAndAssignTrainerCommand, Result<Guid>>
{
    private readonly IUnitOfWork _uow;

    public UpgradeAndAssignTrainerHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<Guid>> Handle(
        UpgradeAndAssignTrainerCommand cmd, CancellationToken ct)
    {
        // ── 1. Load Data ───────────────────────────────────────────────────
        var trainee = await _uow.Trainees.GetByIdAsync(cmd.TraineeId, ct);
        if (trainee is null) return Result<Guid>.NotFound("Trainee", cmd.TraineeId);

        var plan = await _uow.MembershipPlans.GetByIdAsync(cmd.NewPlanId, ct);
        if (plan is null || !plan.IsActive || !plan.IncludesPersonalTrainer)
            return Result<Guid>.Failure("Selected plan is invalid or does not include personal trainer access.");

        var trainer = await _uow.Trainers.GetByIdAsync(cmd.TrainerId, ct);
        if (trainer is null || !trainer.IsActive)
            return Result<Guid>.NotFound("Trainer", cmd.TrainerId);

        // ── 2. Handle Existing Membership ──────────────────────────────────
        // Mark all active memberships as superseded
        var actives = await _uow.Memberships.FindAsync(
            m => m.TraineeId == cmd.TraineeId && m.Status == MembershipStatus.Active, ct);

        foreach (var m in actives)
        {
            m.Status = MembershipStatus.Expired; // Or a new status like Superseded if you have it
            m.Notes = (m.Notes ?? "") + $" | Superseded by upgrade to {plan.Name} on {DateTime.UtcNow:d}";
            _uow.Memberships.Update(m);
        }

        // ── 3. Create New Membership ───────────────────────────────────────
        // Calculation: Price of new plan + Addon fee (since we're bundling it)
        var totalAmount = plan.Price + plan.TrainerAddonFee;

        var membership = new Membership
        {
            TraineeId               = cmd.TraineeId,
            PlanId                  = cmd.NewPlanId,
            StartDate               = DateTime.UtcNow.Date,
            EndDate                 = DateTime.UtcNow.Date.AddDays(plan.DurationDays),
            TotalAmount             = totalAmount,
            AmountPaid              = totalAmount,
            Status                  = MembershipStatus.Active,
            TrainerAddonPaid        = true,
            TrainerAddonPaidAt      = DateTime.UtcNow,
            TrainerAddonAmountPaid  = plan.TrainerAddonFee,
            CreatedById             = cmd.AdminId,
            Notes                   = $"Upgraded to {plan.Name} with trainer {trainer.FullName}"
        };

        await _uow.Memberships.AddAsync(membership, ct);

        // ── 4. Record Payment ──────────────────────────────────────────────
        var payment = new Payment
        {
            MembershipId    = membership.Id,
            Amount          = totalAmount,
            Method          = cmd.Method,
            Status          = PaymentStatus.Paid,
            RecordedById    = cmd.AdminId,
            ReferenceNumber = $"UPGR-{Guid.NewGuid():N}"[..14].ToUpper(),
            Notes           = $"Plan Upgrade ({plan.Name}) + Trainer Add-on"
        };
        await _uow.Payments.AddAsync(payment, ct);

        // ── 5. Handle Existing Trainer Assignment ──────────────────────────
        var currentAssignment = await _uow.TrainerAssignments.FirstOrDefaultAsync(
            a => a.TraineeId == cmd.TraineeId && a.RemovedAt == null, ct);

        if (currentAssignment is not null)
        {
            currentAssignment.RemovedAt = DateTime.UtcNow;
            currentAssignment.RemovedById = cmd.AdminId;
            currentAssignment.RemovalReason = "Membership upgraded and reassigned";
            _uow.TrainerAssignments.Update(currentAssignment);
        }

        // ── 6. Create New Assignment ───────────────────────────────────────
        var assignment = new TrainerAssignment
        {
            TraineeId    = cmd.TraineeId,
            TrainerId    = cmd.TrainerId,
            AssignedAt   = DateTime.UtcNow,
            AssignedById = cmd.AdminId
        };
        await _uow.TrainerAssignments.AddAsync(assignment, ct);

        // ── 7. Notify ──────────────────────────────────────────────────────
        if (trainee.ApplicationUserId is not null)
        {
            await _uow.Notifications.AddAsync(new Notification
            {
                UserId = trainee.ApplicationUserId,
                Title  = "Membership Upgraded & Trainer Assigned",
                Body   = $"Your membership has been upgraded to {plan.Name}. {trainer.FullName} is now your personal trainer.",
                Type   = NotificationType.General
            }, ct);
        }

        await _uow.SaveChangesAsync(ct);
        return Result<Guid>.Success(assignment.Id);
    }
}

public class UpgradeAndAssignTrainerValidator : AbstractValidator<UpgradeAndAssignTrainerCommand>
{
    public UpgradeAndAssignTrainerValidator()
    {
        RuleFor(x => x.TraineeId).NotEmpty();
        RuleFor(x => x.NewPlanId).NotEmpty();
        RuleFor(x => x.TrainerId).NotEmpty();
    }
}
