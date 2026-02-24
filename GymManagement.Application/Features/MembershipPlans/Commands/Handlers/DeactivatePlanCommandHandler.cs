using GymManagement.Application.Common.Models;
using GymManagement.Domain.Interfaces;
using MediatR;
using GymManagement.Application._Features.MembershipPlans.Commands.Models;

namespace GymManagement.Application._Features.MembershipPlans.Commands.Handlers;

public class DeactivatePlanCommandHandler(IUnitOfWork uow) : IRequestHandler<DeactivatePlanCommand, Result>
{

    public async Task<Result> Handle(DeactivatePlanCommand cmd, CancellationToken ct)
    {
        var plan = await uow.MembershipPlans.GetByIdAsync(cmd.PlanId, ct);
        if (plan is null)
            return Result.NotFound("MembershipPlan", cmd.PlanId);

        plan.IsActive = false;
        uow.MembershipPlans.Update(plan);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
