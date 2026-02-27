using GymManagement.Application.Common.Behaviours;

namespace GymManagement.Application._Features.Payments.Commands.Models;

public record RefundPaymentCommand(
    Guid PaymentId,
    string Reason,
    string RefundedById
) : ICommand;
