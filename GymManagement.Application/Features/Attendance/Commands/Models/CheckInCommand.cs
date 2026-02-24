using GymManagement.Application.Common.Behaviours;
using GymManagement.Domain.Enums;
using MediatR;

namespace GymManagement.Application._Features.Attendance.Commands.Models;

public record CheckInCommand(
    Guid TraineeId,
    AttendanceMethod Method,
    string RecordedById
) : ICommand<Guid>;
