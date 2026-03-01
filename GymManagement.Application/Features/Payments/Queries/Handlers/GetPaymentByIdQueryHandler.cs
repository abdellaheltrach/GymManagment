using AutoMapper;
using GymManagement.Application.Common.DTOs;
using GymManagement.Application.Features.Payments.Queries.Models;
using GymManagement.Domain.Interfaces;
using GymManagement.Domain.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Application.Features.Payments.Queries.Handlers;

public class GetPaymentByIdQueryHandler(IUnitOfWork uow, IMapper mapper)
    : IRequestHandler<GetPaymentByIdQuery, Result<PaymentDto>>
{
    public async Task<Result<PaymentDto>> Handle(GetPaymentByIdQuery query, CancellationToken ct)
    {
        var payment = await uow.Payments.AsQueryable()
            .Include(p => p.Membership)
                .ThenInclude(m => m.Trainee)
            .Include(p => p.Membership)
                .ThenInclude(m => m.Plan)
            .FirstOrDefaultAsync(p => p.Id == query.PaymentId, ct);

        if (payment is null)
            return Result<PaymentDto>.NotFound("Payment", query.PaymentId);

        return Result<PaymentDto>.Success(mapper.Map<PaymentDto>(payment));
    }
}
