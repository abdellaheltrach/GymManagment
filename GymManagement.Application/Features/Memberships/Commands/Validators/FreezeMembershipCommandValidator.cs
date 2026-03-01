using FluentValidation;
using GymManagement.Application.Features.Memberships.Commands.Models;

namespace GymManagement.Application.Features.Memberships.Commands.Validators;

public class FreezeMembershipCommandValidator : AbstractValidator<FreezeMembershipCommand>
{
    public FreezeMembershipCommandValidator()
    {
        RuleFor(x => x.MembershipId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
        RuleFor(x => x.RequestedById).NotEmpty();
        RuleFor(x => x.FreezeFrom).NotEmpty();
        RuleFor(x => x.FreezeTo)
            .GreaterThan(x => x.FreezeFrom)
            .WithMessage("Freeze end date must be after freeze start date.");
    }
}
