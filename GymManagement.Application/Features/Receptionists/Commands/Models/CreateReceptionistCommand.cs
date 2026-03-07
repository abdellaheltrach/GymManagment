using GymManagement.Application.Common.Behaviours;

namespace GymManagement.Application.Features.Receptionists.Commands.Models
{
    public record CreateReceptionistCommand(
        string FirstName,
        string LastName,
        string Email,
        string Phone,
        string Password,
        int Permissions,
        string? CreatedById = null) : ICommand<Guid>;
}
