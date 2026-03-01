using GymManagement.Application.Features.Attendance.Commands.Models;
using GymManagement.Domain.Enums;
using GymManagement.Domain.Interfaces;
using GymManagement.Domain.Results;
using MediatR;

namespace GymManagement.Application.Features.Attendance.Commands.Handlers;

public class CheckInCommandHandler(IUnitOfWork uow) : IRequestHandler<CheckInCommand, Result<Guid>>
{

    public async Task<Result<Guid>> Handle(CheckInCommand cmd, CancellationToken ct)
    {
        var trainee = await uow.Trainees.GetByIdAsync(cmd.TraineeId, ct);
        if (trainee is null)
            return Result<Guid>.NotFound("Trainee", cmd.TraineeId);

        // Domain guard: must have Active membership to enter
        var hasActiveMembership = await uow.Memberships.AnyAsync(
            m => m.TraineeId == cmd.TraineeId &&
                 m.Status == MembershipStatus.Active &&
                 m.EndDate > DateTime.UtcNow, ct);

        if (!hasActiveMembership)
            return Result<Guid>.Failure(
                "Trainee does not have an active membership. Access denied.");

        // Guard: no open check-in today
        var today = DateTime.UtcNow.Date;
        var openCheckIn = await uow.Attendances.AnyAsync(
            a => a.TraineeId == cmd.TraineeId &&
                 a.CheckInTime >= today &&
                 a.CheckOutTime == null, ct);

        if (openCheckIn)
            return Result<Guid>.Conflict("Trainee already has an open check-in today.");

        var attendance = new GymManagement.Domain.Entities.Attendance
        {
            TraineeId = cmd.TraineeId,
            CheckInTime = DateTime.UtcNow,
            Method = cmd.Method,
            RecordedById = cmd.RecordedById
        };

        await uow.Attendances.AddAsync(attendance, ct);
        await uow.SaveChangesAsync(ct);

        return Result<Guid>.Success(attendance.Id);
    }
}
