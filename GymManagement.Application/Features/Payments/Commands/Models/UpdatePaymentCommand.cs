using GymManagement.Application.Common.Behaviours;
using GymManagement.Domain.Enums;
using GymManagement.Domain.Results;

namespace GymManagement.Application.Features.Payments.Commands.Models;

public record UpdatePaymentCommand(
    Guid PaymentId,
    decimal AdditionalAmount,
    PaymentMethod Method,
    string UpdatedById,
    string? Notes = null
) : ICommand;
