using FluentValidation;
using GymManagement.Application._Features.Trainers.Commands.Models;

namespace GymManagement.Application._Features.Trainers.Commands.Validators;

public class RemoveTrainerCommandValidator : AbstractValidator<RemoveTrainerCommand>
{
    public RemoveTrainerCommandValidator()
    {
        RuleFor(x => x.TraineeId).NotEmpty();
        RuleFor(x => x.RemovedById).NotEmpty();
    }
}
