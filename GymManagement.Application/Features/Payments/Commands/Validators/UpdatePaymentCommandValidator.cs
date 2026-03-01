using FluentValidation;
using GymManagement.Application.Features.Payments.Commands.Models;

namespace GymManagement.Application.Features.Payments.Commands.Validators;

public class UpdatePaymentCommandValidator : AbstractValidator<UpdatePaymentCommand>
{
    public UpdatePaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId).NotEmpty();
        RuleFor(x => x.AdditionalAmount).GreaterThan(0);
        RuleFor(x => x.Method).IsInEnum();
        RuleFor(x => x.UpdatedById).NotEmpty();
    }
}
