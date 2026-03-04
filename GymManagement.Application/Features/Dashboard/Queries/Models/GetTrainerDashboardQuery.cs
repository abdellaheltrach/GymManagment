using GymManagement.Application.Common.Behaviours;
using GymManagement.Application.Common.DTOs;

namespace GymManagement.Application.Features.Dashboard.Queries.Models
{
    public record GetTrainerDashboardQuery(Guid TrainerId) : IQuery<TrainerDashboardDto>;

}
