using FluentValidation;
using GymManagement.Application.Features.MembershipPlans.Commands.Models;

namespace GymManagement.Application.Features.MembershipPlans.Commands.Validators;

public class CreatePlanCommandValidator : AbstractValidator<CreatePlanCommand>
{
    public CreatePlanCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DurationDays).GreaterThan(0);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.MaxFreezeDays).GreaterThanOrEqualTo(0);
    }
}
