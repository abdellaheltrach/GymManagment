using GymManagement.Application._Features.Memberships.Commands.Models;
using GymManagement.Domain.Entities;
using GymManagement.Domain.Enums;
using GymManagement.Domain.Interfaces;
using GymManagement.Domain.Results;
using MediatR;

namespace GymManagement.Application._Features.Memberships.Commands.Handlers;

public class AssignMembershipCommandHandler(IUnitOfWork uow) : IRequestHandler<AssignMembershipCommand, Result<Guid>>
{

    public async Task<Result<Guid>> Handle(AssignMembershipCommand cmd, CancellationToken ct)
    {
        // Guard: trainee exists
        var trainee = await uow.Trainees.GetByIdAsync(cmd.TraineeId, ct);
        if (trainee is null)
            return Result<Guid>.NotFound("Trainee", cmd.TraineeId);

        // Guard: no currently active membership
        var hasActive = await uow.Memberships.AnyAsync(
            m => m.TraineeId == cmd.TraineeId &&
                 m.Status == MembershipStatus.Active, ct);

        if (hasActive)
            return Result<Guid>.Conflict("Trainee already has an active membership.");

        // Guard: plan exists and is active
        var plan = await uow.MembershipPlans.GetByIdAsync(cmd.PlanId, ct);
        if (plan is null || !plan.IsActive)
            return Result<Guid>.NotFound("MembershipPlan", cmd.PlanId);

        // Guard: payment does not exceed plan price
        if (cmd.InitialPaymentAmount > plan.Price)
            return Result<Guid>.Failure(
                $"Payment amount ({cmd.InitialPaymentAmount:C}) exceeds plan price ({plan.Price:C}).");

        // Create membership
        var membership = new Membership
        {
            TraineeId = cmd.TraineeId,
            PlanId = cmd.PlanId,
            StartDate = cmd.StartDate.Date,
            EndDate = cmd.StartDate.Date.AddDays(plan.DurationDays),
            TotalAmount = plan.Price,
            Status = cmd.InitialPaymentAmount >= plan.Price
                            ? MembershipStatus.Active
                            : MembershipStatus.PendingPayment,
            Notes = cmd.Notes,
            CreatedById = cmd.RecordedById
        };

        await uow.Memberships.AddAsync(membership, ct);

        // Record initial payment if provided
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

            membership.AmountPaid = cmd.InitialPaymentAmount;
            await uow.Payments.AddAsync(payment, ct);
        }

        // Welcome notification
        var notification = new Notification
        {
            UserId = trainee.ApplicationUserId ?? cmd.RecordedById,
            Title = "Membership Activated",
            Body = $"Your {plan.Name} membership is active until {membership.EndDate:dd MMM yyyy}.",
            Type = NotificationType.General
        };
        await uow.Notifications.AddAsync(notification, ct);

        await uow.SaveChangesAsync(ct);
        return Result<Guid>.Success(membership.Id);
    }
}
