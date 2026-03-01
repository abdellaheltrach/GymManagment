using GymManagement.Application.Common.Behaviours;
using GymManagement.Application.Common.DTOs;
using GymManagement.Domain.Enums;
using GymManagement.Domain.Results;

namespace GymManagement.Application.Features.Payments.Queries.Models;

public record GetPaymentsListQuery(
    int Page = 1,
    int PageSize = 20,
    string? SearchTerm = null,
    PaymentMethod? MethodFilter = null,
    PaymentStatus? StatusFilter = null
) : IQuery<PagedResult<PaymentDto>>;
