using GymManagement.Application.Features.Payments.Commands.Models;
using GymManagement.Domain.Enums;
using GymManagement.Domain.Interfaces;
using GymManagement.Domain.Results;
using MediatR;

namespace GymManagement.Application.Features.Payments.Commands.Handlers;

public class UpdatePaymentCommandHandler(IUnitOfWork uow) : IRequestHandler<UpdatePaymentCommand, Result>
{
    public async Task<Result> Handle(UpdatePaymentCommand cmd, CancellationToken ct)
    {
        var payment = await uow.Payments.GetByIdAsync(cmd.PaymentId, ct);
        if (payment is null)
            return Result.NotFound("Payment", cmd.PaymentId);

        if (payment.IsRefunded)
            return Result.Failure("Cannot update a refunded payment.");

        var membership = await uow.Memberships.GetByIdAsync(payment.MembershipId, ct);
        if (membership is null)
            return Result.NotFound("Membership", payment.MembershipId);

        if (cmd.AdditionalAmount <= 0)
            return Result.Failure("Additional amount must be greater than zero.");

        if (cmd.AdditionalAmount > membership.RemainingBalance)
            return Result.Failure($"Additional amount ({cmd.AdditionalAmount:C}) exceeds remaining balance ({membership.RemainingBalance:C}).");

        // Update payment
        payment.Amount += cmd.AdditionalAmount;
        payment.Method = cmd.Method;
        payment.Notes = string.IsNullOrWhiteSpace(cmd.Notes) ? payment.Notes : cmd.Notes;
        
        // Update membership
        membership.AmountPaid += cmd.AdditionalAmount;
        membership.UpdatedById = cmd.UpdatedById;

        if (membership.IsFullyPaid)
        {
            payment.Status = PaymentStatus.Paid;
            if (membership.Status == MembershipStatus.PendingPayment)
                membership.Status = MembershipStatus.Active;
        }
        else
        {
            payment.Status = PaymentStatus.PartiallyPaid;
        }

        uow.Payments.Update(payment);
        uow.Memberships.Update(membership);

        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
