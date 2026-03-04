using GymManagement.Application.Common.Behaviours;
using GymManagement.Application.Common.DTOs;
using GymManagement.Domain.Interfaces;
using GymManagement.Domain.Results;
using MediatR;

namespace GymManagement.Application.Features.TrainingPrograms.Queries.GetTrainerPrograms;

public record GetTrainerProgramsQuery(
    Guid TrainerId,
    Guid? TraineeId = null,
    bool ActiveOnly = false
) : IQuery<IReadOnlyList<TrainingProgramDto>>;

public class GetTrainerProgramsHandler
    : IRequestHandler<GetTrainerProgramsQuery,
                      Result<IReadOnlyList<TrainingProgramDto>>>
{
    private readonly IUnitOfWork _uow;
    public GetTrainerProgramsHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<IReadOnlyList<TrainingProgramDto>>> Handle(
        GetTrainerProgramsQuery query, CancellationToken ct)
    {
        var programs = await _uow.TrainingPrograms.FindAsync(
            p => p.TrainerId == query.TrainerId &&
                 (query.TraineeId == null || p.TraineeId == query.TraineeId) &&
                 (!query.ActiveOnly || p.IsActive), ct);

        var result = new List<TrainingProgramDto>();

        foreach (var prog in programs.OrderByDescending(p => p.StartDate))
        {
            var trainee = await _uow.Trainees.GetByIdAsync(prog.TraineeId, ct);
            var trainer = await _uow.Trainers.GetByIdAsync(prog.TrainerId, ct);
            var exercises = await _uow.Exercises.FindAsync(
                e => e.TrainingProgramId == prog.Id, ct);

            var exerciseDtos = exercises
                .OrderBy(e => e.Order)
                .Select(e => new ExerciseDto(
                    e.Id, e.Name, e.Sets, e.Reps, e.WeightKg,
                    e.DurationSeconds, e.RestSeconds, e.Order, e.Notes))
                .ToList().AsReadOnly();

            result.Add(new TrainingProgramDto(
                prog.Id,
                prog.TrainerId,
                trainer?.FullName ?? string.Empty,
                prog.TraineeId,
                trainee?.FullName ?? string.Empty,
                prog.Title,
                prog.Description,
                prog.StartDate,
                prog.EndDate,
                prog.IsActive,
                exerciseDtos));
        }

        return Result<IReadOnlyList<TrainingProgramDto>>
            .Success(result.AsReadOnly());
    }
}
