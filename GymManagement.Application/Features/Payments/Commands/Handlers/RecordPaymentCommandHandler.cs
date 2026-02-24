using GymManagement.Application.Common.Models;
using GymManagement.Domain.Entities;
using GymManagement.Domain.Enums;
using GymManagement.Domain.Interfaces;
using MediatR;
using GymManagement.Application._Features.Payments.Commands.Models;

namespace GymManagement.Application._Features.Payments.Commands.Handlers;

public class RecordPaymentCommandHandler(IUnitOfWork uow) : IRequestHandler<RecordPaymentCommand, Result<Guid>>
{

    public async Task<Result<Guid>> Handle(RecordPaymentCommand cmd, CancellationToken ct)
    {
        var membership = await uow.Memberships.GetByIdAsync(cmd.MembershipId, ct);
        if (membership is null)
            return Result<Guid>.NotFound("Membership", cmd.MembershipId);

        if (membership.Status == MembershipStatus.Expired)
            return Result<Guid>.Failure("Cannot record payment for an expired membership.");

        if (membership.IsFullyPaid)
            return Result<Guid>.Conflict("Membership is already fully paid.");

        if (cmd.Amount > membership.RemainingBalance)
            return Result<Guid>.Failure(
                $"Payment amount ({cmd.Amount:C}) exceeds remaining balance ({membership.RemainingBalance:C}).");

        // Create payment record
        var payment = new Payment
        {
            MembershipId    = cmd.MembershipId,
            Amount          = cmd.Amount,
            Method          = cmd.Method,
            Status          = PaymentStatus.Paid,
            RecordedById    = cmd.RecordedById,
            Notes           = cmd.Notes,
            ReferenceNumber = $"PAY-{Guid.NewGuid():N}"[..12].ToUpper()
        };

        await uow.Payments.AddAsync(payment, ct);

        // Update membership balance — atomic with payment creation
        membership.AmountPaid += cmd.Amount;
        membership.UpdatedById = cmd.RecordedById;

        if (membership.IsFullyPaid && membership.Status == MembershipStatus.PendingPayment)
            membership.Status = MembershipStatus.Active;

        uow.Memberships.Update(membership);

        // Payment received notification
        var trainee = await uow.Trainees.GetByIdAsync(membership.TraineeId, ct);
        if (trainee?.ApplicationUserId is not null)
        {
            await uow.Notifications.AddAsync(new Notification
            {
                UserId = trainee.ApplicationUserId,
                Title  = "Payment Received",
                Body   = $"Payment of {cmd.Amount:C} received. Remaining balance: {membership.RemainingBalance:C}.",
                Type   = NotificationType.PaymentReceived
            }, ct);
        }

        await uow.SaveChangesAsync(ct);
        return Result<Guid>.Success(payment.Id);
    }
}
