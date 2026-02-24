using FluentValidation;
using GymManagement.Application._Features.Trainees.Commands.Models;

namespace GymManagement.Application._Features.Trainees.Commands.Validators;

public class AnonymiseTraineeCommandValidator : AbstractValidator<AnonymiseTraineeCommand>
{
    public AnonymiseTraineeCommandValidator()
    {
        RuleFor(x => x.TraineeId).NotEmpty();
        RuleFor(x => x.RequestedById).NotEmpty();
    }
}
