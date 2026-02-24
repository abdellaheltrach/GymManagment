using FluentValidation;
using GymManagement.Application._Features.Attendance.Commands.Models;

namespace GymManagement.Application._Features.Attendance.Commands.Validators;

public class CheckOutCommandValidator : AbstractValidator<CheckOutCommand>
{
    public CheckOutCommandValidator()
    {
        RuleFor(x => x.TraineeId).NotEmpty();
        RuleFor(x => x.RecordedById).NotEmpty();
    }
}
