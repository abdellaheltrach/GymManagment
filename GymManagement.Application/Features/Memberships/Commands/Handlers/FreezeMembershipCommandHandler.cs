using GymManagement.Application.Common.Models;
using GymManagement.Domain.Entities;
using GymManagement.Domain.Enums;
using GymManagement.Domain.Interfaces;
using MediatR;
using GymManagement.Application._Features.Memberships.Commands.Models;

namespace GymManagement.Application._Features.Memberships.Commands.Handlers;

public class FreezeMembershipCommandHandler(IUnitOfWork uow) : IRequestHandler<FreezeMembershipCommand, Result<Guid>>
{

    public async Task<Result<Guid>> Handle(FreezeMembershipCommand cmd, CancellationToken ct)
    {
        var membership = await uow.Memberships.GetByIdAsync(cmd.MembershipId, ct);
        if (membership is null)
            return Result<Guid>.NotFound("Membership", cmd.MembershipId);

        if (membership.Status != MembershipStatus.Active)
            return Result<Guid>.Failure("Only active memberships can be frozen.");

        // Check plan allows freezing
        var plan = await uow.MembershipPlans.GetByIdAsync(membership.PlanId, ct);
        if (plan is null || plan.MaxFreezeDays == 0)
            return Result<Guid>.Failure("This membership plan does not allow freezing.");

        // Check freeze days remaining
        var requestedDays = (cmd.FreezeTo - cmd.FreezeFrom).Days;
        var remainingFreezeDays = plan.MaxFreezeDays - membership.TotalFrozenDays;

        if (requestedDays > remainingFreezeDays)
            return Result<Guid>.Failure(
                $"Cannot freeze for {requestedDays} days. Only {remainingFreezeDays} freeze days remaining.");

        // Check no existing active freeze
        var existingFreeze = await uow.FrozenPeriods.AnyAsync(
            f => f.MembershipId == cmd.MembershipId &&
                 f.FrozenTo     > DateTime.UtcNow, ct);

        if (existingFreeze)
            return Result<Guid>.Conflict("Membership already has an active freeze period.");

        var frozenPeriod = new FrozenPeriod
        {
            MembershipId = cmd.MembershipId,
            FrozenFrom   = cmd.FreezeFrom,
            FrozenTo     = cmd.FreezeTo,
            Reason       = cmd.Reason,
            RequestedById = cmd.RequestedById
        };

        await uow.FrozenPeriods.AddAsync(frozenPeriod, ct);

        membership.Status    = MembershipStatus.Frozen;
        membership.UpdatedById = cmd.RequestedById;
        uow.Memberships.Update(membership);

        await uow.SaveChangesAsync(ct);
        return Result<Guid>.Success(frozenPeriod.Id);
    }
}
