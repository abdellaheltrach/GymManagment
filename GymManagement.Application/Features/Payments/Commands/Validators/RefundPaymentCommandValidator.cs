using FluentValidation;
using GymManagement.Application.Features.Payments.Commands.Models;

namespace GymManagement.Application.Features.Payments.Commands.Validators;

public class RefundPaymentCommandValidator : AbstractValidator<RefundPaymentCommand>
{
    public RefundPaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
        RuleFor(x => x.RefundedById).NotEmpty();
    }
}
