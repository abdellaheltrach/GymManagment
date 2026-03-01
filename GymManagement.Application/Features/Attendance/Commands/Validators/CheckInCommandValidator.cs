using FluentValidation;
using GymManagement.Application.Features.Attendance.Commands.Models;

namespace GymManagement.Application.Features.Attendance.Commands.Validators;

public class CheckInCommandValidator : AbstractValidator<CheckInCommand>
{
    public CheckInCommandValidator()
    {
        RuleFor(x => x.TraineeId).NotEmpty();
        RuleFor(x => x.RecordedById).NotEmpty();
    }
}
