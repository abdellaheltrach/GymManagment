using FluentValidation;
using GymManagement.Application.Features.Memberships.Commands.Models;

namespace GymManagement.Application.Features.Memberships.Commands.Validators;

public class RenewMembershipCommandValidator : AbstractValidator<RenewMembershipCommand>
{
    public RenewMembershipCommandValidator()
    {
        RuleFor(x => x.TraineeId).NotEmpty();
        RuleFor(x => x.PlanId).NotEmpty();
        RuleFor(x => x.InitialPaymentAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.RecordedById).NotEmpty();
    }
}
