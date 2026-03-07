using GymManagement.Application.Common.Behaviours;

namespace GymManagement.Application.Features.Receptionists.Commands.Models
{
    public record UpdateReceptionistCommand(
        Guid Id,
        string FirstName,
        string LastName,
        string Phone) : ICommand;
}
