using FluentValidation;
using GymManagement.Application.Features.Progress.Commands.Models;

namespace GymManagement.Application.Features.Progress.Commands.Validators;

public class RecordProgressCommandValidator : AbstractValidator<RecordProgressCommand>
{
    public RecordProgressCommandValidator()
    {
        RuleFor(x => x.TraineeId).NotEmpty();
        RuleFor(x => x.RecordedById).NotEmpty();
        RuleFor(x => x.WeightKg).InclusiveBetween(20, 500).When(x => x.WeightKg.HasValue);
        RuleFor(x => x.BodyFatPercent).InclusiveBetween(2, 70).When(x => x.BodyFatPercent.HasValue);
    }
}
