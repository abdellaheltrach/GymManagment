using GymManagement.Application.Common.Behaviours;
using GymManagement.Application.Common.DTOs;
using GymManagement.Domain.Enums;
using GymManagement.Domain.Results;

namespace GymManagement.Application.Features.Payments.Queries.Models;

public record GetPaymentsListQuery(
    int Page = 1,
    int PageSize = 20,
    PaymentMethod? MethodFilter = null
) : IQuery<PagedResult<PaymentDto>>;
