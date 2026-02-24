using AutoMapper;
using GymManagement.Application.Common.DTOs;
using GymManagement.Application.Common.Models;
using GymManagement.Domain.Interfaces;
using MediatR;
using GymManagement.Application._Features.MembershipPlans.Queries.Models;

namespace GymManagement.Application._Features.MembershipPlans.Queries.Handlers;

public class GetActivePlansQueryHandler(IUnitOfWork uow, IMapper mapper)
    : IRequestHandler<GetActivePlansQuery, Result<IReadOnlyList<MembershipPlanDto>>>
{

    public async Task<Result<IReadOnlyList<MembershipPlanDto>>> Handle(
        GetActivePlansQuery query, CancellationToken ct)
    {
        var plans = await uow.MembershipPlans.FindAsync(p => p.IsActive, ct);
        var dtos  = plans.Select(p => mapper.Map<MembershipPlanDto>(p)).ToList();
        return Result<IReadOnlyList<MembershipPlanDto>>.Success(dtos);
    }
}
