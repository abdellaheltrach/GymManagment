using FluentValidation;
using GymManagement.Application.Common.Behaviours;
using GymManagement.Domain.Entities;
using GymManagement.Domain.Enums;
using GymManagement.Domain.Interfaces;
using GymManagement.Domain.Results;
using MediatR;

namespace GymManagement.Application.Features.Memberships.Commands.PayTrainerAddon;

/// <summary>
/// Records the one-time personal trainer add-on payment for the current
/// membership cycle.
///
/// BUSINESS RULES:
///   1. Trainee must have an active membership.
///   2. The plan must include IncludesPersonalTrainer = true.
///   3. The add-on must not already be paid for this cycle.
///   4. If TrainerAddonFee == 0 the plan includes the trainer at no extra cost —
///      we mark it paid automatically for zero (admin just confirms).
///   5. Payment amount must exactly match TrainerAddonFee (no partials on add-ons).
///   6. Creates a Payment record for the audit trail, stamps TrainerAddonPaid = true
///      and TrainerAddonPaidAt on the Membership, then saves atomically.
///
/// After this command succeeds, AssignOrReassignTrainerCommand will allow
/// assignment without re-checking payment again for this cycle.
/// </summary>
public record PayTrainerAddonCommand(
    Guid          TraineeId,
    decimal       AmountPaid,
    PaymentMethod Method,
    string        RecordedById,
    string?       Notes
) : ICommand<Guid>; // returns the Payment.Id

public class PayTrainerAddonHandler
    : IRequestHandler<PayTrainerAddonCommand, Result<Guid>>
{
    private readonly IUnitOfWork _uow;

    public PayTrainerAddonHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<Guid>> Handle(
        PayTrainerAddonCommand cmd, CancellationToken ct)
    {
        // ── 1. Load active membership ──────────────────────────────────────
        var memberships = await _uow.Memberships.FindAsync(
            m => m.TraineeId == cmd.TraineeId &&
                 m.Status    == MembershipStatus.Active &&
                 m.EndDate   >  DateTime.UtcNow, ct);

        var membership = memberships.FirstOrDefault();

        if (membership is null)
            return Result<Guid>.Failure(
                "Trainee does not have an active membership. " +
                "A trainer add-on can only be purchased with an active subscription.");

        // ── 2. Load plan ───────────────────────────────────────────────────
        var plan = await _uow.MembershipPlans.GetByIdAsync(membership.PlanId, ct);

        if (plan is null || !plan.IncludesPersonalTrainer)
            return Result<Guid>.Failure(
                $"The plan '{plan?.Name ?? "Unknown"}' does not support personal trainer access. " +
                "Please upgrade to a plan that includes this feature.");

        // ── 3. Guard: already paid this cycle ──────────────────────────────
        if (membership.TrainerAddonPaid)
            return Result<Guid>.Conflict(
                "Personal trainer add-on has already been paid for this membership cycle. " +
                "You can now assign or change trainers at no extra cost.");

        // ── 4. Validate payment amount ─────────────────────────────────────
        // If the fee is 0 the trainer is included at no extra cost.
        // We still create a zero-amount payment record so the event is auditable.
        if (plan.TrainerAddonFee > 0 && cmd.AmountPaid != plan.TrainerAddonFee)
            return Result<Guid>.Failure(
                $"Trainer add-on fee is {plan.TrainerAddonFee:C}. " +
                $"Amount provided ({cmd.AmountPaid:C}) does not match. " +
                "Partial payment is not allowed for add-ons.");

        // ── 5. Create payment record ───────────────────────────────────────
        var payment = new Payment
        {
            MembershipId    = membership.Id,
            Amount          = cmd.AmountPaid,
            Method          = cmd.Method,
            Status          = PaymentStatus.Paid,
            RecordedById    = cmd.RecordedById,
            Notes           = cmd.Notes ?? "Personal trainer add-on fee",
            ReferenceNumber = $"ADDON-{Guid.NewGuid():N}"[..14].ToUpper()
        };

        await _uow.Payments.AddAsync(payment, ct);

        // ── 6. Mark add-on paid on the membership ──────────────────────────
        membership.TrainerAddonPaid       = true;
        membership.TrainerAddonPaidAt     = DateTime.UtcNow;
        membership.TrainerAddonAmountPaid = cmd.AmountPaid;
        membership.UpdatedById            = cmd.RecordedById;
        _uow.Memberships.Update(membership);

        // ── 7. Notification ────────────────────────────────────────────────
        var trainee = await _uow.Trainees.GetByIdAsync(cmd.TraineeId, ct);
        if (trainee?.ApplicationUserId is not null)
        {
            await _uow.Notifications.AddAsync(new Notification
            {
                UserId = trainee.ApplicationUserId,
                Title  = "Personal Trainer Access Unlocked",
                Body   = plan.TrainerAddonFee > 0
                    ? $"Your trainer add-on payment of {cmd.AmountPaid:C} has been received. " +
                      "A personal trainer can now be assigned to you."
                    : "Personal trainer access has been activated for your membership.",
                Type   = NotificationType.General
            }, ct);
        }

        await _uow.SaveChangesAsync(ct);
        return Result<Guid>.Success(payment.Id);
    }
}

public class PayTrainerAddonValidator : AbstractValidator<PayTrainerAddonCommand>
{
    public PayTrainerAddonValidator()
    {
        RuleFor(x => x.TraineeId).NotEmpty();
        RuleFor(x => x.AmountPaid).GreaterThanOrEqualTo(0);
        RuleFor(x => x.RecordedById).NotEmpty();
    }
}
