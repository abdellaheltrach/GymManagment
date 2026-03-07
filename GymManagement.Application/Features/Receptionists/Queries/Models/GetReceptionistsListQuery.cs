using GymManagement.Application.Common.Behaviours;
using GymManagement.Application.Common.DTOs;
using GymManagement.Domain.Results;

namespace GymManagement.Application.Features.Receptionists.Queries.Models
{
    public record GetReceptionistsListQuery(
        int Page,
        int PageSize,
        string? Search = null,
        bool? IsActive = null) : IQuery<PagedResult<ReceptionistSummaryDto>>;
}
