using AutoMapper;
using GymManagement.Application.Common.DTOs;
using GymManagement.Application.Features.Trainers.Queries.Models;
using GymManagement.Domain.Interfaces;
using GymManagement.Domain.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Application.Features.Trainers.Queries.Handlers
{
    public class GetTrainersListQueryHandler(IUnitOfWork uow, IMapper mapper)
        : IRequestHandler<GetTrainersListQuery, Result<PagedResult<TrainerSummaryDto>>>
    {
        public async Task<Result<PagedResult<TrainerSummaryDto>>> Handle(GetTrainersListQuery request, CancellationToken ct)
        {
            var query = uow.Trainers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var lowerSearch = request.Search.ToLower();
                query = query.Where(t =>
                    t.FirstName.ToLower().Contains(lowerSearch) ||
                    t.LastName.ToLower().Contains(lowerSearch) ||
                    t.Email.ToLower().Contains(lowerSearch));
            }

            if (request.Specialization.HasValue)
            {
                query = query.Where(t => t.Specialization == request.Specialization.Value);
            }

            var totalCount = await query.CountAsync(ct);
            var items = await query
                .OrderBy(t => t.LastName)
                .ThenBy(t => t.FirstName)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(ct);

            var dtos = mapper.Map<IReadOnlyList<TrainerSummaryDto>>(items);

            return Result<PagedResult<TrainerSummaryDto>>.Success(
                new PagedResult<TrainerSummaryDto>(dtos, totalCount, request.Page, request.PageSize));
        }
    }
}
