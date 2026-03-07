using GymManagement.Application.Features.Receptionists.Commands.Models;
using GymManagement.Domain.Interfaces;
using GymManagement.Domain.Results;
using MediatR;

namespace GymManagement.Application.Features.Receptionists.Commands.Handlers
{
    public class UpdateReceptionistPermissionsCommandHandler(
        IUnitOfWork uow) : IRequestHandler<UpdateReceptionistPermissionsCommand, Result>
    {
        public async Task<Result> Handle(UpdateReceptionistPermissionsCommand cmd, CancellationToken ct)
        {
            var receptionist = await uow.Receptionists.GetByIdAsync(cmd.Id, ct);
            if (receptionist is null)
                return Result.NotFound("Receptionist", cmd.Id);

            receptionist.Permissions = cmd.Permissions;

            return Result.Success();
        }
    }
}
