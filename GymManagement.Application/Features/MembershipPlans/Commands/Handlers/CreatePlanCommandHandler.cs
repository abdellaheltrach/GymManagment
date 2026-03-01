using GymManagement.Application.Features.MembershipPlans.Commands.Models;
using GymManagement.Domain.Entities;
using GymManagement.Domain.Interfaces;
using GymManagement.Domain.Results;
using MediatR;

namespace GymManagement.Application.Features.MembershipPlans.Commands.Handlers;

public class CreatePlanCommandHandler(IUnitOfWork uow) : IRequestHandler<CreatePlanCommand, Result<Guid>>
{

    public async Task<Result<Guid>> Handle(CreatePlanCommand cmd, CancellationToken ct)
    {
        var exists = await uow.MembershipPlans.AnyAsync(p => p.Name == cmd.Name, ct);
        if (exists)
            return Result<Guid>.Conflict($"A plan named '{cmd.Name}' already exists.");

        var plan = new MembershipPlan
        {
            Name = cmd.Name,
            Description = cmd.Description,
            DurationDays = cmd.DurationDays,
            Price = cmd.Price,
            AccessLevel = cmd.AccessLevel,
            IncludesPersonalTrainer = cmd.IncludesPersonalTrainer,
            MaxFreezeDays = cmd.MaxFreezeDays,
            IsActive = true
        };

        await uow.MembershipPlans.AddAsync(plan, ct);
        await uow.SaveChangesAsync(ct);
        return Result<Guid>.Success(plan.Id);
    }
}
