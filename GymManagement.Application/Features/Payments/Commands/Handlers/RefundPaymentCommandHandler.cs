using GymManagement.Application._Features.Payments.Commands.Models;
using GymManagement.Domain.Enums;
using GymManagement.Domain.Interfaces;
using GymManagement.Domain.Results;
using MediatR;

namespace GymManagement.Application._Features.Payments.Commands.Handlers;

public class RefundPaymentCommandHandler(IUnitOfWork uow) : IRequestHandler<RefundPaymentCommand, Result>
{

    public async Task<Result> Handle(RefundPaymentCommand cmd, CancellationToken ct)
    {
        var payment = await uow.Payments.GetByIdAsync(cmd.PaymentId, ct);
        if (payment is null)
            return Result.NotFound("Payment", cmd.PaymentId);

        if (payment.IsRefunded)
            return Result.Conflict("Payment has already been refunded.");

        var membership = await uow.Memberships.GetByIdAsync(payment.MembershipId, ct);
        if (membership is null)
            return Result.NotFound("Membership", payment.MembershipId);

        // Mark payment as refunded
        payment.IsRefunded = true;
        payment.RefundedAt = DateTime.UtcNow;
        payment.RefundedById = cmd.RefundedById;
        payment.RefundReason = cmd.Reason;
        payment.Status = PaymentStatus.Refunded;
        uow.Payments.Update(payment);

        // Subtract from membership balance
        membership.AmountPaid -= payment.Amount;
        membership.UpdatedById = cmd.RefundedById;

        // If membership was Active and is now under-paid, revert to PendingPayment
        if (!membership.IsFullyPaid && membership.Status == MembershipStatus.Active)
            membership.Status = MembershipStatus.PendingPayment;

        uow.Memberships.Update(membership);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
