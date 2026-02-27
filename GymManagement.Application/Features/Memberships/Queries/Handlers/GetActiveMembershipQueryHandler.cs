using AutoMapper;
using GymManagement.Application._Features.Memberships.Queries.Models;
using GymManagement.Application.Common.DTOs;
using GymManagement.Domain.Enums;
using GymManagement.Domain.Interfaces;
using GymManagement.Domain.Results;
using MediatR;

namespace GymManagement.Application._Features.Memberships.Queries.Handlers;

public class GetActiveMembershipQueryHandler(IUnitOfWork uow, IMapper mapper)
    : IRequestHandler<GetActiveMembershipQuery, Result<MembershipDto>>
{

    public async Task<Result<MembershipDto>> Handle(
        GetActiveMembershipQuery query, CancellationToken ct)
    {
        var membership = (await uow.Memberships.FindAsync(
            m => m.TraineeId == query.TraineeId &&
                 m.Status == MembershipStatus.Active &&
                 m.EndDate > DateTime.UtcNow, ct))
            .FirstOrDefault();

        if (membership is null)
            return Result<MembershipDto>.NotFound(
                $"No active membership found for trainee '{query.TraineeId}'.");

        // Load plan for mapping
        var plan = await uow.MembershipPlans.GetByIdAsync(membership.PlanId, ct);
        membership.Plan = plan!;

        return Result<MembershipDto>.Success(mapper.Map<MembershipDto>(membership));
    }
}
