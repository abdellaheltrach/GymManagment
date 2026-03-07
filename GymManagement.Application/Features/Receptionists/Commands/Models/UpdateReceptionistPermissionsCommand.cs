using GymManagement.Application.Common.Behaviours;

namespace GymManagement.Application.Features.Receptionists.Commands.Models
{
    public record UpdateReceptionistPermissionsCommand(
        Guid Id,
        int Permissions) : ICommand;
}
