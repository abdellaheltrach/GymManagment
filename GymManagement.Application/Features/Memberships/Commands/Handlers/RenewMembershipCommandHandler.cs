using GymManagement.Application._Features.Memberships.Commands.Models;
using GymManagement.Domain.Entities;
using GymManagement.Domain.Enums;
using GymManagement.Domain.Interfaces;
using GymManagement.Domain.Results;
using MediatR;

namespace GymManagement.Application._Features.Memberships.Commands.Handlers;

public class RenewMembershipCommandHandler(IUnitOfWork uow) : IRequestHandler<RenewMembershipCommand, Result<Guid>>
{

    public async Task<Result<Guid>> Handle(RenewMembershipCommand cmd, CancellationToken ct)
    {
        var plan = await uow.MembershipPlans.GetByIdAsync(cmd.PlanId, ct);
        if (plan is null || !plan.IsActive)
            return Result<Guid>.NotFound("MembershipPlan", cmd.PlanId);

        // Find the most recent membership — must be Expired or expiring within 7 days
        var recent = (await uow.Memberships.FindAsync(
            m => m.TraineeId == cmd.TraineeId, ct))
            .OrderByDescending(m => m.EndDate)
            .FirstOrDefault();

        if (recent is not null)
        {
            var isExpired = recent.Status == MembershipStatus.Expired;
            var expiringSoon = recent.EndDate <= DateTime.UtcNow.AddDays(7);

            if (!isExpired && !expiringSoon)
                return Result<Guid>.Conflict(
                    "Membership can only be renewed if it is expired or expiring within 7 days.");
        }

        // New membership starts from day after old EndDate (or today if none)
        var startDate = recent?.EndDate.AddDays(1).Date ?? DateTime.UtcNow.Date;

        var membership = new Membership
        {
            TraineeId = cmd.TraineeId,
            PlanId = cmd.PlanId,
            StartDate = startDate,
            EndDate = startDate.AddDays(plan.DurationDays),
            TotalAmount = plan.Price,
            AmountPaid = cmd.InitialPaymentAmount,
            Status = cmd.InitialPaymentAmount >= plan.Price
                            ? MembershipStatus.Active
                            : MembershipStatus.PendingPayment,
            CreatedById = cmd.RecordedById
        };

        await uow.Memberships.AddAsync(membership, ct);

        if (cmd.InitialPaymentAmount > 0)
        {
            var payment = new Payment
            {
                MembershipId = membership.Id,
                Amount = cmd.InitialPaymentAmount,
                Method = cmd.PaymentMethod,
                Status = PaymentStatus.Paid,
                RecordedById = cmd.RecordedById,
                ReferenceNumber = $"PAY-{Guid.NewGuid():N}"[..12].ToUpper()
            };
            await uow.Payments.AddAsync(payment, ct);
        }

        await uow.SaveChangesAsync(ct);
        return Result<Guid>.Success(membership.Id);
    }
}
