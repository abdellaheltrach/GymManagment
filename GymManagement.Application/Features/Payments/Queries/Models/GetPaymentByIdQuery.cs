using GymManagement.Application.Common.Behaviours;
using GymManagement.Application.Common.DTOs;
using GymManagement.Domain.Results;

namespace GymManagement.Application.Features.Payments.Queries.Models;

public record GetPaymentByIdQuery(Guid PaymentId) : IQuery<PaymentDto>;
