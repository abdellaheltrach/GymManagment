using GymManagement.Application.Common.Behaviours;

namespace GymManagement.Application.Features.Receptionists.Commands.Models
{
    public record ReactivateReceptionistCommand(Guid Id) : ICommand;
}
