using FluentValidation;
using GymManagement.Application.Features.Receptionists.Commands.Models;

namespace GymManagement.Application.Features.Receptionists.Commands.Validators
{
    public class CreateReceptionistValidator : AbstractValidator<CreateReceptionistCommand>
    {
        public CreateReceptionistValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(50);

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(50);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email address.");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Phone number is required.")
                .MaximumLength(20);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(12).WithMessage("Password must be at least 12 characters.");
        }
    }
}
