using FluentValidation;
using GymManagement.Application.Features.Trainees.Commands.Models;

namespace GymManagement.Application.Features.Trainees.Commands.Validators;

public class AnonymiseTraineeCommandValidator : AbstractValidator<AnonymiseTraineeCommand>
{
    public AnonymiseTraineeCommandValidator()
    {
        RuleFor(x => x.TraineeId).NotEmpty();
        RuleFor(x => x.RequestedById).NotEmpty();
    }
}
