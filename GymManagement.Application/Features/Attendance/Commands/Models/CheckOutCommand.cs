using GymManagement.Application.Common.Behaviours;

namespace GymManagement.Application._Features.Attendance.Commands.Models;

public record CheckOutCommand(Guid TraineeId, string RecordedById) : ICommand;
