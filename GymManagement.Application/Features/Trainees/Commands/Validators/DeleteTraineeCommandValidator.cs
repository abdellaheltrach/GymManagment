using FluentValidation;
using GymManagement.Application._Features.Trainees.Commands.Models;

namespace GymManagement.Application._Features.Trainees.Commands.Validators;

public class DeleteTraineeCommandValidator : AbstractValidator<DeleteTraineeCommand>
{
    public DeleteTraineeCommandValidator()
    {
        RuleFor(x => x.TraineeId).NotEmpty();
        RuleFor(x => x.DeletedById).NotEmpty();
    }
}
