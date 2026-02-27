using GymManagement.Application.Common.Behaviours;

namespace GymManagement.Application._Features.MembershipPlans.Commands.Models;

public record DeactivatePlanCommand(Guid PlanId) : ICommand;
