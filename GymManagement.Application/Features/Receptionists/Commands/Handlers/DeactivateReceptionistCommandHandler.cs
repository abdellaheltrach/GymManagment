using GymManagement.Application.Features.Receptionists.Commands.Models;
using GymManagement.Domain.Interfaces;
using GymManagement.Domain.Results;
using MediatR;

namespace GymManagement.Application.Features.Receptionists.Commands.Handlers
{
    public class DeactivateReceptionistCommandHandler(
        IUnitOfWork uow) : IRequestHandler<DeactivateReceptionistCommand, Result>
    {
        public async Task<Result> Handle(DeactivateReceptionistCommand cmd, CancellationToken ct)
        {
            var receptionist = await uow.Receptionists.GetByIdAsync(cmd.Id, ct);
            if (receptionist is null)
                return Result.NotFound("Receptionist", cmd.Id);

            if (!receptionist.IsActive)
                return Result.Failure("Receptionist is already deactivated.");

            receptionist.IsActive = false;

            return Result.Success();
        }
    }
}
