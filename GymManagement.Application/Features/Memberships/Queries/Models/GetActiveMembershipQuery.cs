using GymManagement.Application.Common.Behaviours;
using GymManagement.Application.Common.DTOs;

namespace GymManagement.Application.Features.Memberships.Queries.Models;

public record GetActiveMembershipQuery(Guid TraineeId) : IQuery<MembershipDto>;
