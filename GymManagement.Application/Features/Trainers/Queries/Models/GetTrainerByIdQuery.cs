using GymManagement.Application.Common.DTOs;
using GymManagement.Domain.Results;
using MediatR;

namespace GymManagement.Application.Features.Trainers.Queries.Models
{
    public record GetTrainerByIdQuery(Guid Id) : IRequest<Result<TrainerDetailDto>>;
}
