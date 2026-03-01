using GymManagement.Application.Common.Behaviours;

namespace GymManagement.Application.Features.Memberships.Commands.Models;

public record FreezeMembershipCommand(
    Guid MembershipId,
    DateTime FreezeFrom,
    DateTime FreezeTo,
    string Reason,
    string RequestedById
) : ICommand<Guid>;
