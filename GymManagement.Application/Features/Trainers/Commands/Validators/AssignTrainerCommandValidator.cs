using FluentValidation;
using GymManagement.Application._Features.Trainers.Commands.Models;

namespace GymManagement.Application._Features.Trainers.Commands.Validators;

public class AssignTrainerCommandValidator : AbstractValidator<AssignTrainerCommand>
{
    public AssignTrainerCommandValidator()
    {
        RuleFor(x => x.TrainerId).NotEmpty();
        RuleFor(x => x.TraineeId).NotEmpty();
        RuleFor(x => x.AssignedById).NotEmpty();
    }
}
