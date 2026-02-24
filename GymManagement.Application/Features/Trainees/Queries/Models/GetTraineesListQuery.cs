using GymManagement.Application.Common.Behaviours;
using GymManagement.Application.Common.DTOs;
using GymManagement.Application.Common.Models;
using GymManagement.Domain.Enums;
using MediatR;

namespace GymManagement.Application._Features.Trainees.Queries.Models;

public record GetTraineesListQuery(
    int Page = 1,
    int PageSize = 20,
    string? SearchTerm = null,
    MembershipStatus? StatusFilter = null
) : IQuery<PagedResult<TraineeSummaryDto>>;
