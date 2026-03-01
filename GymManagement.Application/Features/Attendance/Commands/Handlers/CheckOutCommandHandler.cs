using GymManagement.Application.Features.Attendance.Commands.Models;
using GymManagement.Domain.Interfaces;
using GymManagement.Domain.Results;
using MediatR;

namespace GymManagement.Application.Features.Attendance.Commands.Handlers;

public class CheckOutCommandHandler(IUnitOfWork uow) : IRequestHandler<CheckOutCommand, Result>
{

    public async Task<Result> Handle(CheckOutCommand cmd, CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;

        var openAttendance = await uow.Attendances.FirstOrDefaultAsync(
            a => a.TraineeId == cmd.TraineeId &&
                 a.CheckInTime >= today &&
                 a.CheckOutTime == null, ct);

        if (openAttendance is null)
            return Result.Failure("No open check-in found for this trainee today.");

        openAttendance.CheckOutTime = DateTime.UtcNow;
        openAttendance.RecordedById = cmd.RecordedById;
        uow.Attendances.Update(openAttendance);

        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
