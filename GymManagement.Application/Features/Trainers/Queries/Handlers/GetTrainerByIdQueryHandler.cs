using AutoMapper;
using GymManagement.Application.Common.DTOs;
using GymManagement.Application.Features.Trainers.Queries.Models;
using GymManagement.Domain.Interfaces;
using GymManagement.Domain.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Application.Features.Trainers.Queries.Handlers
{
    public class GetTrainerByIdQueryHandler(IUnitOfWork uow, IMapper mapper)
        : IRequestHandler<GetTrainerByIdQuery, Result<TrainerDetailDto>>
    {
        public async Task<Result<TrainerDetailDto>> Handle(GetTrainerByIdQuery request, CancellationToken ct)
        {
            var trainer = await uow.Trainers.AsQueryable()
                .Include(t => t.TrainerAssignments)
                    .ThenInclude(ta => ta.Trainee) // This might need adjustment based on navigation properties
                .FirstOrDefaultAsync(t => t.Id == request.Id, ct);

            if (trainer is null)
                return Result<TrainerDetailDto>.NotFound($"Trainer with ID {request.Id} not found.");

            return Result<TrainerDetailDto>.Success(mapper.Map<TrainerDetailDto>(trainer));
        }
    }
}
