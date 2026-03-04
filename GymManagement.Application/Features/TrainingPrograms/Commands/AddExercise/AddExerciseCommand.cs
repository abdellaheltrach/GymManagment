using FluentValidation;
using GymManagement.Application.Common.Behaviours;
using GymManagement.Domain.Entities;
using GymManagement.Domain.Interfaces;
using GymManagement.Domain.Results;
using MediatR;

namespace GymManagement.Application.Features.TrainingPrograms.Commands.AddExercise;

public record AddExerciseCommand(
    Guid ProgramId,
    Guid TrainerId,   // for ownership check
    string Name,
    int Sets,
    int Reps,
    decimal? WeightKg,
    int? DurationSeconds,
    int? RestSeconds,
    string? Notes
) : ICommand<Guid>;

public class AddExerciseHandler
    : IRequestHandler<AddExerciseCommand, Result<Guid>>
{
    private readonly IUnitOfWork _uow;
    public AddExerciseHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<Guid>> Handle(
        AddExerciseCommand cmd, CancellationToken ct)
    {
        var program = await _uow.TrainingPrograms.GetByIdAsync(cmd.ProgramId, ct);
        if (program is null)
            return Result<Guid>.NotFound("TrainingProgram", cmd.ProgramId);

        if (program.TrainerId != cmd.TrainerId)
            return Result<Guid>.Forbidden(
                "You do not own this training program.");

        // Auto-order: put new exercise at the end
        var existing = await _uow.Exercises.FindAsync(
            e => e.TrainingProgramId == cmd.ProgramId, ct);
        var nextOrder = existing.Count > 0
            ? existing.Max(e => e.Order) + 1 : 1;

        var exercise = new Exercise
        {
            TrainingProgramId = cmd.ProgramId,
            Name = cmd.Name,
            Sets = cmd.Sets,
            Reps = cmd.Reps,
            WeightKg = cmd.WeightKg,
            DurationSeconds = cmd.DurationSeconds,
            RestSeconds = cmd.RestSeconds,
            Notes = cmd.Notes,
            Order = nextOrder
        };

        await _uow.Exercises.AddAsync(exercise, ct);
        await _uow.SaveChangesAsync(ct);

        return Result<Guid>.Success(exercise.Id);
    }
}

public class AddExerciseValidator : AbstractValidator<AddExerciseCommand>
{
    public AddExerciseValidator()
    {
        RuleFor(x => x.ProgramId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Sets).InclusiveBetween(1, 100);
        RuleFor(x => x.Reps).InclusiveBetween(1, 1000);
        RuleFor(x => x.WeightKg)
            .GreaterThan(0).When(x => x.WeightKg.HasValue);
    }
}
