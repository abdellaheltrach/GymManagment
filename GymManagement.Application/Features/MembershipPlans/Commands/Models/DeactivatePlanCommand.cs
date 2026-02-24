using GymManagement.Application.Common.Behaviours;
using MediatR;

namespace GymManagement.Application._Features.MembershipPlans.Commands.Models;

public record DeactivatePlanCommand(Guid PlanId) : ICommand;
