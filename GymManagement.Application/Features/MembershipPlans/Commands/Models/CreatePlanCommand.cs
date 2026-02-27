using GymManagement.Application.Common.Behaviours;
using GymManagement.Domain.Enums;

namespace GymManagement.Application._Features.MembershipPlans.Commands.Models;

public record CreatePlanCommand(
    string Name,
    string? Description,
    int DurationDays,
    decimal Price,
    AccessLevel AccessLevel,
    bool IncludesPersonalTrainer,
    int MaxFreezeDays
) : ICommand<Guid>;
