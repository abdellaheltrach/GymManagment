using GymManagement.Application.Common.Behaviours;

namespace GymManagement.Application.Features.MembershipPlans.Commands.Models;

public record DeactivatePlanCommand(Guid PlanId) : ICommand;
