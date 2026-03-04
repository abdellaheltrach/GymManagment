using GymManagement.Application.Common.Behaviours;
using GymManagement.Application.Common.DTOs;
using GymManagement.Domain.Enums;

namespace GymManagement.Application.Features.Trainers.Queries.Models
{
    public record GetMyTraineesQuery(
        Guid TrainerId,
        string? SearchTerm = null,
        MembershipStatus? StatusFilter = null
    ) : IQuery<IReadOnlyList<MyTraineeSummaryDto>>;


}
