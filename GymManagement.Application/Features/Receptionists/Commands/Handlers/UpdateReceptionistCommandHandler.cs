using GymManagement.Application.Features.Receptionists.Commands.Models;
using GymManagement.Domain.Interfaces;
using GymManagement.Domain.Results;
using MediatR;

namespace GymManagement.Application.Features.Receptionists.Commands.Handlers
{
    public class UpdateReceptionistCommandHandler(
        IUnitOfWork uow) : IRequestHandler<UpdateReceptionistCommand, Result>
    {
        public async Task<Result> Handle(UpdateReceptionistCommand cmd, CancellationToken ct)
        {
            var receptionist = await uow.Receptionists.GetByIdAsync(cmd.Id, ct);
            if (receptionist is null)
                return Result.NotFound("Receptionist", cmd.Id);

            receptionist.FirstName = cmd.FirstName;
            receptionist.LastName = cmd.LastName;
            receptionist.Phone = cmd.Phone;

            return Result.Success();
        }
    }
}
