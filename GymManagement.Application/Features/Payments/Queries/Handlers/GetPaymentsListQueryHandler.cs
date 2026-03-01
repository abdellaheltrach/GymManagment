using AutoMapper;
using GymManagement.Application.Common.DTOs;
using GymManagement.Application.Features.Payments.Queries.Models;
using GymManagement.Domain.Entities;
using GymManagement.Domain.Interfaces;
using GymManagement.Domain.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Application.Features.Payments.Queries.Handlers;

public class GetPaymentsListQueryHandler(IUnitOfWork uow, IMapper mapper)
    : IRequestHandler<GetPaymentsListQuery, Result<PagedResult<PaymentDto>>>
{
    public async Task<Result<PagedResult<PaymentDto>>> Handle(GetPaymentsListQuery query, CancellationToken ct)
    {
        IQueryable<Payment> paymentsQuery = uow.Payments.AsQueryable()
            .Include(p => p.Membership)
                .ThenInclude(m => m.Trainee)
            .Include(p => p.Membership)
                .ThenInclude(m => m.Plan);

        if (query.MethodFilter.HasValue)
        {
            paymentsQuery = paymentsQuery.Where(p => p.Method == query.MethodFilter.Value);
        }

        var totalCount = await paymentsQuery.CountAsync(ct);
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var pagedPayments = await paymentsQuery
            .OrderByDescending(p => p.PaidAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var dtos = mapper.Map<List<PaymentDto>>(pagedPayments);

        return Result<PagedResult<PaymentDto>>.Success(
            new PagedResult<PaymentDto>(dtos, totalCount, page, pageSize));
    }
}
