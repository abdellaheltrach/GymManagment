using GymManagement.Application.Common.Behaviours;
using GymManagement.Application.Common.DTOs;

namespace GymManagement.Application.Features.MembershipPlans.Queries.Models;

public record GetActivePlansQuery : IQuery<IReadOnlyList<MembershipPlanDto>>;
