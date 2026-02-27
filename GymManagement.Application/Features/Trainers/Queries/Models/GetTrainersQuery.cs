using GymManagement.Application.Common.DTOs;
using GymManagement.Domain.Enums;
using GymManagement.Domain.Results;
using MediatR;

namespace GymManagement.Application.Features.Trainers.Queries.Models
{
    public record GetTrainersListQuery(
        int Page,
        int PageSize,
        string? Search = null,
        TrainerSpecialization? Specialization = null) : IRequest<Result<PagedResult<TrainerSummaryDto>>>;
}
