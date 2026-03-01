using FluentValidation;
using GymManagement.Application.Features.Memberships.Commands.Models;

namespace GymManagement.Application.Features.Memberships.Commands.Validators;

public class AssignMembershipCommandValidator : AbstractValidator<AssignMembershipCommand>
{
    public AssignMembershipCommandValidator()
    {
        RuleFor(x => x.TraineeId).NotEmpty();
        RuleFor(x => x.PlanId).NotEmpty();
        RuleFor(x => x.StartDate).NotEmpty().GreaterThanOrEqualTo(DateTime.UtcNow.Date.AddDays(-1))
            .WithMessage("Start date cannot be in the past.");
        RuleFor(x => x.InitialPaymentAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.RecordedById).NotEmpty();
    }
}
