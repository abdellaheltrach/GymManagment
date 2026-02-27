using GymManagement.Application.Common.Behaviours;
using GymManagement.Domain.Enums;

namespace GymManagement.Application._Features.Memberships.Commands.Models;

public record RenewMembershipCommand(
    Guid TraineeId,
    Guid PlanId,
    decimal InitialPaymentAmount,
    PaymentMethod PaymentMethod,
    string RecordedById
) : ICommand<Guid>;
