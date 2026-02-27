using GymManagement.Application.Common.Behaviours;
using GymManagement.Domain.Enums;

namespace GymManagement.Application._Features.Payments.Commands.Models;

public record RecordPaymentCommand(
    Guid MembershipId,
    decimal Amount,
    PaymentMethod Method,
    string RecordedById,
    string? Notes
) : ICommand<Guid>;
