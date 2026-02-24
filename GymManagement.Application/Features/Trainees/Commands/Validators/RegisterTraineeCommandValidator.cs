using FluentValidation;
using GymManagement.Application._Features.Trainees.Commands.Models;

namespace GymManagement.Application._Features.Trainees.Commands.Validators;

public class RegisterTraineeCommandValidator : AbstractValidator<RegisterTraineeCommand>
{
    public RegisterTraineeCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty().EmailAddress().WithMessage("A valid email is required.")
            .MaximumLength(256);

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is required.")
            .MaximumLength(20);

        RuleFor(x => x.NationalId)
            .NotEmpty().WithMessage("National ID is required.")
            .MaximumLength(50);

        RuleFor(x => x.DateOfBirth)
            .NotEmpty()
            .Must(dob => DateTime.UtcNow.Year - dob.Year >= 16)
            .WithMessage("Trainee must be at least 16 years old.");

        RuleFor(x => x.EmergencyContactName)
            .NotEmpty().WithMessage("Emergency contact name is required.")
            .MaximumLength(100);

        RuleFor(x => x.EmergencyContactPhone)
            .NotEmpty().WithMessage("Emergency contact phone is required.")
            .MaximumLength(20);

        RuleFor(x => x.HeightCm)
            .InclusiveBetween(50, 300).When(x => x.HeightCm.HasValue)
            .WithMessage("Height must be between 50 and 300 cm.");

        RuleFor(x => x.WeightKg)
            .InclusiveBetween(20, 500).When(x => x.WeightKg.HasValue)
            .WithMessage("Weight must be between 20 and 500 kg.");
    }
}
