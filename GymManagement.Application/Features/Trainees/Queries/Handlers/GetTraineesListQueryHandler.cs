using GymManagement.Application.Features.Trainees.Queries.Models;
using GymManagement.Application.Common.DTOs;
using GymManagement.Domain.Enums;
using GymManagement.Domain.Interfaces;
using GymManagement.Domain.Results;
using MediatR;

namespace GymManagement.Application.Features.Trainees.Queries.Handlers;

public class GetTraineesListQueryHandler(IUnitOfWork uow)
    : IRequestHandler<GetTraineesListQuery, Result<PagedResult<TraineeSummaryDto>>>
{
    public async Task<Result<PagedResult<TraineeSummaryDto>>> Handle(
        GetTraineesListQuery query, CancellationToken ct)
    {
        var allTrainees = await uow.Trainees.GetAllAsync(ct);

        // Filter by search term
        var filtered = allTrainees.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.ToLower();
            filtered = filtered.Where(t =>
                t.FullName.ToLower().Contains(term) ||
                t.Email.ToLower().Contains(term) ||
                t.Phone.Contains(term));
        }

        // Filter by membership status requires joining with memberships
        if (query.StatusFilter.HasValue)
        {
            var memberships = await uow.Memberships.FindAsync(
                m => m.Status == query.StatusFilter.Value, ct);

            var traineeIdsWithStatus = memberships
                .Select(m => m.TraineeId)
                .ToHashSet();

            filtered = filtered.Where(t => traineeIdsWithStatus.Contains(t.Id));
        }

        var list = filtered.ToList();
        var totalCount = list.Count;
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var paged = list
            .OrderBy(t => t.LastName).ThenBy(t => t.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // Build summary DTOs — get active membership for each in one batch
        var membershipsAll = await uow.Memberships.FindAsync(
            m => paged.Select(t => t.Id).Contains(m.TraineeId) &&
                 m.Status == MembershipStatus.Active, ct);

        var membershipMap = membershipsAll.ToDictionary(m => m.TraineeId);

        var dtos = paged.Select(t =>
        {
            membershipMap.TryGetValue(t.Id, out var membership);
            return new TraineeSummaryDto(
                t.Id,
                t.FullName,
                t.Email,
                t.Phone,
                t.JoinDate,
                membership?.Status,
                membership?.EndDate
            );
        }).ToList();

        return Result<PagedResult<TraineeSummaryDto>>.Success(
            new PagedResult<TraineeSummaryDto>(dtos, totalCount, page, pageSize));
    }
}

