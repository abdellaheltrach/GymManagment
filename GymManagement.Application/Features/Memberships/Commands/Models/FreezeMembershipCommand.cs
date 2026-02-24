using GymManagement.Application.Common.Behaviours;
using MediatR;

namespace GymManagement.Application._Features.Memberships.Commands.Models;

public record FreezeMembershipCommand(
    Guid MembershipId,
    DateTime FreezeFrom,
    DateTime FreezeTo,
    string Reason,
    string RequestedById
) : ICommand<Guid>;
