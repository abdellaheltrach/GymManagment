using FluentValidation;
using GymManagement.Application.Features.Payments.Commands.Models;

namespace GymManagement.Application.Features.Payments.Commands.Validators;

public class RecordPaymentCommandValidator : AbstractValidator<RecordPaymentCommand>
{
    public RecordPaymentCommandValidator()
    {
        RuleFor(x => x.MembershipId).NotEmpty();
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Payment amount must be greater than zero.");
        RuleFor(x => x.RecordedById).NotEmpty();
    }
}
