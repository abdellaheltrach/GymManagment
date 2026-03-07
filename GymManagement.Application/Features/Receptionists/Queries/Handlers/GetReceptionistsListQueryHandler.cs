using GymManagement.Application.Common.DTOs;
using GymManagement.Application.Features.Receptionists.Queries.Models;
using GymManagement.Domain.Interfaces;
using GymManagement.Domain.Results;
using MediatR;

namespace GymManagement.Application.Features.Receptionists.Queries.Handlers
{
    public class GetReceptionistsListQueryHandler(
       IUnitOfWork uow) : IRequestHandler<GetReceptionistsListQuery, Result<PagedResult<ReceptionistSummaryDto>>>
    {
        public async Task<Result<PagedResult<ReceptionistSummaryDto>>> Handle(
            GetReceptionistsListQuery query, CancellationToken ct)
        {
            var all = await uow.Receptionists.GetAllAsync(ct);

            var filtered = all.AsEnumerable();

            if (query.IsActive.HasValue)
                filtered = filtered.Where(r => r.IsActive == query.IsActive.Value);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var s = query.Search.Trim().ToLower();
                filtered = filtered.Where(r =>
                    r.FullName.ToLower().Contains(s) ||
                    r.Email.ToLower().Contains(s) ||
                    r.Phone.Contains(s));
            }

            var ordered = filtered
                .OrderByDescending(r => r.IsActive)
                .ThenBy(r => r.LastName)
                .ThenBy(r => r.FirstName)
                .ToList();

            var total = ordered.Count;

            var items = ordered
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(r => new ReceptionistSummaryDto(
                    r.Id,
                    r.FullName,
                    r.Email,
                    r.Phone,
                    r.HireDate,
                    r.IsActive,
                    (Domain.Enums.ReceptionistPermission)r.Permissions))
                .ToList();

            return Result<PagedResult<ReceptionistSummaryDto>>.Success(
                new PagedResult<ReceptionistSummaryDto>(items, query.Page, query.PageSize, total));
        }
    }
}