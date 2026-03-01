using FluentValidation;
using GymManagement.Application.Features.Trainees.Commands.Models;

namespace GymManagement.Application.Features.Trainees.Commands.Validators;

public class DeleteTraineeCommandValidator : AbstractValidator<DeleteTraineeCommand>
{
    public DeleteTraineeCommandValidator()
    {
        RuleFor(x => x.TraineeId).NotEmpty();
        RuleFor(x => x.DeletedById).NotEmpty();
    }
}
