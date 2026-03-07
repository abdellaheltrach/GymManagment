using GymManagement.Application.Common.Behaviours;
using GymManagement.Application.Common.DTOs;

namespace GymManagement.Application.Features.Receptionists.Queries.Models
{
    public record GetReceptionistByIdQuery(Guid Id) : IQuery<ReceptionistDetailDto>;
}
