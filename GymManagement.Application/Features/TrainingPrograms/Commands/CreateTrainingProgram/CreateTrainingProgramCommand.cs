using FluentValidation;
using GymManagement.Application.Common.Behaviours;
using GymManagement.Domain.Entities;
using GymManagement.Domain.Interfaces;
using GymManagement.Domain.Results;
using MediatR;

namespace GymManagement.Application.Features.TrainingPrograms.Commands.CreateTrainingProgram;

public record CreateTrainingProgramCommand(
    Guid TrainerId,
    Guid TraineeId,
    string Title,
    string? Description,
    DateTime StartDate,
    DateTime? EndDate
) : ICommand<Guid>;

public class CreateTrainingProgramHandler
    : IRequestHandler<CreateTrainingProgramCommand, Result<Guid>>
{
    private readonly IUnitOfWork _uow;
    public CreateTrainingProgramHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<Guid>> Handle(
        CreateTrainingProgramCommand cmd, CancellationToken ct)
    {
        // Guard: trainee must be assigned to this trainer
        var isAssigned = await _uow.TrainerAssignments.AnyAsync(
            a => a.TrainerId == cmd.TrainerId &&
                 a.TraineeId == cmd.TraineeId &&
                 a.RemovedAt == null, ct);

        if (!isAssigned)
            return Result<Guid>.Forbidden(
                "Trainee is not assigned to this trainer.");

        var program = new TrainingProgram
        {
            TrainerId = cmd.TrainerId,
            TraineeId = cmd.TraineeId,
            Title = cmd.Title,
            Description = cmd.Description,
            StartDate = cmd.StartDate,
            EndDate = cmd.EndDate,
            IsActive = true
        };

        await _uow.TrainingPrograms.AddAsync(program, ct);
        await _uow.SaveChangesAsync(ct);

        return Result<Guid>.Success(program.Id);
    }
}

public class CreateTrainingProgramValidator
    : AbstractValidator<CreateTrainingProgramCommand>
{
    public CreateTrainingProgramValidator()
    {
        RuleFor(x => x.TrainerId).NotEmpty();
        RuleFor(x => x.TraineeId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .When(x => x.EndDate.HasValue)
            .WithMessage("End date must be after start date.");
    }
}
