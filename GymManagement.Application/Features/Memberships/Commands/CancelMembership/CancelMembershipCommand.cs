using FluentValidation;
using GymManagement.Application.Common.Behaviours;
using GymManagement.Domain.Entities;
using GymManagement.Domain.Enums;
using GymManagement.Domain.Interfaces;
using GymManagement.Domain.Results;
using MediatR;

namespace GymManagement.Application.Features.Memberships.Commands.CancelMembership;

public enum RefundType { None, Full, Partial }

/// <summary>
/// Cancels an active (or frozen/pending) membership.
///
/// WHAT THIS DOES (in one SaveChangesAsync):
///   1. Sets Membership.Status = Cancelled.
///   2. If RefundType.Full   → marks all non-refunded payments as IsRefunded=true,
///      sums them up, creates one outbound refund Payment record.
///   3. If RefundType.Partial → creates one refund Payment for RefundAmount.
///      Does NOT touch individual payment records (partial is discretionary).
///   4. RefundType.None → just cancels, no money movement.
///   5. AUTO-REMOVES the active trainer assignment (soft-remove, RemovedAt stamped).
///   6. Sends a notification to the trainee.
///
/// CASH REFUND NOTE: If any payment was made by cash, the system still records
/// the refund entry for audit. A note is appended: "Cash refund — handle manually."
/// </summary>
public record CancelMembershipCommand(
    Guid       MembershipId,
    RefundType RefundType,
    decimal    RefundAmount,   // 0 if RefundType.None or Full (Full is auto-calculated)
    PaymentMethod RefundMethod,
    string     Reason,
    string     CancelledById
) : ICommand;

public class CancelMembershipHandler
    : IRequestHandler<CancelMembershipCommand, Result>
{
    private readonly IUnitOfWork _uow;

    public CancelMembershipHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(CancelMembershipCommand cmd, CancellationToken ct)
    {
        // ── 1. Load membership ─────────────────────────────────────────────
        var membership = await _uow.Memberships.GetByIdAsync(cmd.MembershipId, ct);
        if (membership is null)
            return Result.NotFound("Membership", cmd.MembershipId);

        var cancellableStatuses = new[]
        {
            MembershipStatus.Active,
            MembershipStatus.Frozen,
            MembershipStatus.PendingPayment,
            MembershipStatus.Suspended
        };

        if (!cancellableStatuses.Contains(membership.Status))
            return Result.Failure(
                $"Cannot cancel a membership with status '{membership.Status}'. " +
                "Only Active, Frozen, Pending, or Suspended memberships can be cancelled.");

        // ── 2. Compute refund ──────────────────────────────────────────────
        decimal actualRefundAmount = 0;
        string  refundNotes        = cmd.Reason;

        if (cmd.RefundType == RefundType.Full)
        {
            // Load all non-refunded payments
            var payments = await _uow.Payments.FindAsync(
                p => p.MembershipId == cmd.MembershipId && !p.IsRefunded, ct);

            actualRefundAmount = payments.Sum(p => p.Amount);

            if (actualRefundAmount > 0)
            {
                // Mark each original payment as refunded
                foreach (var p in payments)
                {
                    p.IsRefunded  = true;
                    p.RefundedAt  = DateTime.UtcNow;
                    p.RefundedById = cmd.CancelledById;
                    p.RefundReason = cmd.Reason;
                    _uow.Payments.Update(p);
                }

                // Check if any were cash — flag manual handling
                bool hasCash = payments.Any(p => p.Method == PaymentMethod.Cash);
                if (hasCash)
                    refundNotes += " | CASH REFUND — handle manually with member.";

                // Create a single consolidated refund record
                await _uow.Payments.AddAsync(new Payment
                {
                    MembershipId    = cmd.MembershipId,
                    Amount          = -actualRefundAmount, // negative = outbound
                    Method          = cmd.RefundMethod,
                    Status          = PaymentStatus.Refunded,
                    RecordedById    = cmd.CancelledById,
                    Notes           = refundNotes,
                    ReferenceNumber = $"REF-{Guid.NewGuid():N}"[..12].ToUpper(),
                    IsRefunded      = false // this IS the refund record
                }, ct);
            }
        }
        else if (cmd.RefundType == RefundType.Partial)
        {
            if (cmd.RefundAmount <= 0)
                return Result.Failure("Partial refund amount must be greater than zero.");

            if (cmd.RefundAmount > membership.AmountPaid)
                return Result.Failure(
                    $"Refund amount ({cmd.RefundAmount:C}) cannot exceed " +
                    $"total amount paid ({membership.AmountPaid:C}).");

            actualRefundAmount = cmd.RefundAmount;

            bool hasCash = (await _uow.Payments.FindAsync(
                p => p.MembershipId == cmd.MembershipId && !p.IsRefunded
                  && p.Method == PaymentMethod.Cash, ct)).Any();

            if (hasCash)
                refundNotes += " | Includes cash payments — partial cash refund must be handled manually.";

            await _uow.Payments.AddAsync(new Payment
            {
                MembershipId    = cmd.MembershipId,
                Amount          = -actualRefundAmount,
                Method          = cmd.RefundMethod,
                Status          = PaymentStatus.Refunded,
                RecordedById    = cmd.CancelledById,
                Notes           = refundNotes,
                ReferenceNumber = $"REF-{Guid.NewGuid():N}"[..12].ToUpper(),
                IsRefunded      = false
            }, ct);
        }

        // ── 3. Cancel the membership ───────────────────────────────────────
        membership.Status    = MembershipStatus.Cancelled;
        membership.Notes     = string.IsNullOrWhiteSpace(membership.Notes)
            ? $"Cancelled: {cmd.Reason}"
            : membership.Notes + $" | Cancelled: {cmd.Reason}";
        membership.UpdatedById = cmd.CancelledById;
        _uow.Memberships.Update(membership);

        // ── 4. Auto-remove trainer assignment ─────────────────────────────
        var assignment = await _uow.TrainerAssignments.FirstOrDefaultAsync(
            a => a.TraineeId == membership.TraineeId && a.RemovedAt == null, ct);

        if (assignment is not null)
        {
            assignment.RemovedAt     = DateTime.UtcNow;
            assignment.RemovedById   = cmd.CancelledById;
            assignment.RemovalReason = "Membership cancelled";
            _uow.TrainerAssignments.Update(assignment);
        }

        // ── 5. Notification ────────────────────────────────────────────────
        var trainee = await _uow.Trainees.GetByIdAsync(membership.TraineeId, ct);
        if (trainee?.ApplicationUserId is not null)
        {
            var body = cmd.RefundType switch
            {
                RefundType.Full    => $"Your membership has been cancelled. A full refund of {actualRefundAmount:C} has been processed.",
                RefundType.Partial => $"Your membership has been cancelled. A partial refund of {actualRefundAmount:C} has been processed.",
                _                  => "Your membership has been cancelled."
            };

            await _uow.Notifications.AddAsync(new Notification
            {
                UserId = trainee.ApplicationUserId,
                Title  = "Membership Cancelled",
                Body   = body,
                Type   = NotificationType.General
            }, ct);
        }

        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class CancelMembershipValidator : AbstractValidator<CancelMembershipCommand>
{
    public CancelMembershipValidator()
    {
        RuleFor(x => x.MembershipId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
        RuleFor(x => x.CancelledById).NotEmpty();
        RuleFor(x => x.RefundAmount)
            .GreaterThan(0)
            .When(x => x.RefundType == RefundType.Partial)
            .WithMessage("Refund amount is required for partial refunds.");
    }
}
