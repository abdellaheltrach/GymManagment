using GymManagement.Application.Common.Behaviours;
using GymManagement.Domain.Enums;
using MediatR;

namespace GymManagement.Application._Features.Memberships.Commands.Models;

public record AssignMembershipCommand(
    Guid TraineeId,
    Guid PlanId,
    DateTime StartDate,
    decimal InitialPaymentAmount,
    PaymentMethod PaymentMethod,
    string RecordedById,
    string? Notes
) : ICommand<Guid>;
