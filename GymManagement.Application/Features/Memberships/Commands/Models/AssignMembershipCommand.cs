using GymManagement.Application.Common.Behaviours;
using GymManagement.Domain.Enums;

namespace GymManagement.Application.Features.Memberships.Commands.Models;

public record AssignMembershipCommand(
    Guid TraineeId,
    Guid PlanId,
    DateTime StartDate,
    decimal InitialPaymentAmount,
    PaymentMethod PaymentMethod,
    string RecordedById,
    string? Notes
) : ICommand<Guid>;
