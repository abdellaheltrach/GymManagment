using FluentValidation;
using GymManagement.Application._Features.Trainees.Commands.Models;

namespace GymManagement.Application._Features.Trainees.Commands.Validators;

public class UpdateTraineeCommandValidator : AbstractValidator<UpdateTraineeCommand>
{
    public UpdateTraineeCommandValidator()
    {
        RuleFor(x => x.TraineeId).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(20);
        RuleFor(x => x.EmergencyContactName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EmergencyContactPhone).NotEmpty().MaximumLength(20);
        RuleFor(x => x.HeightCm)
            .InclusiveBetween(50, 300).When(x => x.HeightCm.HasValue);
        RuleFor(x => x.WeightKg)
            .InclusiveBetween(20, 500).When(x => x.WeightKg.HasValue);
    }
}
