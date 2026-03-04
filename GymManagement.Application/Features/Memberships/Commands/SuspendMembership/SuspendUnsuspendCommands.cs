using FluentValidation;
using GymManagement.Application.Common.Behaviours;
using GymManagement.Domain.Entities;
using GymManagement.Domain.Enums;
using GymManagement.Domain.Interfaces;
using GymManagement.Domain.Results;
using MediatR;

namespace GymManagement.Application.Features.Memberships.Commands.SuspendMembership;

// ── Suspend ────────────────────────────────────────────────────────────────────

/// <summary>
/// Suspends an active (or frozen) membership.
/// Does NOT move any money and does NOT remove the trainer assignment.
/// The member loses check-in access immediately.
/// Can be reversed with UnsuspendMembershipCommand.
/// </summary>
public record SuspendMembershipCommand(
    Guid   MembershipId,
    string Reason,
    string SuspendedById
) : ICommand;

public class SuspendMembershipHandler
    : IRequestHandler<SuspendMembershipCommand, Result>
{
    private readonly IUnitOfWork _uow;
    public SuspendMembershipHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(SuspendMembershipCommand cmd, CancellationToken ct)
    {
        var membership = await _uow.Memberships.GetByIdAsync(cmd.MembershipId, ct);
        if (membership is null)
            return Result.NotFound("Membership", cmd.MembershipId);

        if (membership.Status == MembershipStatus.Suspended)
            return Result.Conflict("Membership is already suspended.");

        if (membership.Status is MembershipStatus.Expired or MembershipStatus.Cancelled)
            return Result.Failure(
                $"Cannot suspend a membership with status '{membership.Status}'.");

        membership.Status      = MembershipStatus.Suspended;
        membership.Notes       = string.IsNullOrWhiteSpace(membership.Notes)
            ? $"Suspended: {cmd.Reason}"
            : membership.Notes + $" | Suspended: {cmd.Reason}";
        membership.UpdatedById = cmd.SuspendedById;
        _uow.Memberships.Update(membership);

        // Notification
        var trainee = await _uow.Trainees.GetByIdAsync(membership.TraineeId, ct);
        if (trainee?.ApplicationUserId is not null)
        {
            await _uow.Notifications.AddAsync(new Notification
            {
                UserId = trainee.ApplicationUserId,
                Title  = "Membership Suspended",
                Body   = $"Your membership has been suspended. Reason: {cmd.Reason}. Please contact us to resolve.",
                Type   = NotificationType.AccountSuspended
            }, ct);
        }

        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class SuspendMembershipValidator : AbstractValidator<SuspendMembershipCommand>
{
    public SuspendMembershipValidator()
    {
        RuleFor(x => x.MembershipId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
        RuleFor(x => x.SuspendedById).NotEmpty();
    }
}

// ── Unsuspend ──────────────────────────────────────────────────────────────────

/// <summary>
/// Restores a suspended membership back to Active.
/// Access is re-granted immediately.
/// Trainer assignment is NOT auto-restored — must be reassigned if needed.
/// </summary>
public record UnsuspendMembershipCommand(
    Guid   MembershipId,
    string Notes,
    string UnsuspendedById
) : ICommand;

public class UnsuspendMembershipHandler
    : IRequestHandler<UnsuspendMembershipCommand, Result>
{
    private readonly IUnitOfWork _uow;
    public UnsuspendMembershipHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(UnsuspendMembershipCommand cmd, CancellationToken ct)
    {
        var membership = await _uow.Memberships.GetByIdAsync(cmd.MembershipId, ct);
        if (membership is null)
            return Result.NotFound("Membership", cmd.MembershipId);

        if (membership.Status != MembershipStatus.Suspended)
            return Result.Failure(
                $"Membership is not suspended (current status: {membership.Status}).");

        if (membership.EndDate <= DateTime.UtcNow)
            return Result.Failure(
                "Cannot unsuspend — this membership has already expired. " +
                "Please renew the membership instead.");

        membership.Status      = MembershipStatus.Active;
        membership.Notes       = string.IsNullOrWhiteSpace(membership.Notes)
            ? $"Unsuspended: {cmd.Notes}"
            : membership.Notes + $" | Unsuspended: {cmd.Notes}";
        membership.UpdatedById = cmd.UnsuspendedById;
        _uow.Memberships.Update(membership);

        // Notification
        var trainee = await _uow.Trainees.GetByIdAsync(membership.TraineeId, ct);
        if (trainee?.ApplicationUserId is not null)
        {
            await _uow.Notifications.AddAsync(new Notification
            {
                UserId = trainee.ApplicationUserId,
                Title  = "Membership Reactivated",
                Body   = "Your membership has been reactivated. You can now access the gym again.",
                Type   = NotificationType.AccountActivated
            }, ct);
        }

        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class UnsuspendMembershipValidator : AbstractValidator<UnsuspendMembershipCommand>
{
    public UnsuspendMembershipValidator()
    {
        RuleFor(x => x.MembershipId).NotEmpty();
        RuleFor(x => x.UnsuspendedById).NotEmpty();
    }
}
