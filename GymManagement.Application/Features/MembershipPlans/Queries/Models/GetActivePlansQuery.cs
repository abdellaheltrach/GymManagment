using GymManagement.Application.Common.Behaviours;
using GymManagement.Application.Common.DTOs;
using MediatR;

namespace GymManagement.Application._Features.MembershipPlans.Queries.Models;

public record GetActivePlansQuery : IQuery<IReadOnlyList<MembershipPlanDto>>;
