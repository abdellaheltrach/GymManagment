using GymManagement.Application.Common.Behaviours;

namespace GymManagement.Application._Features.Trainees.Commands.Models;

public record DeleteTraineeCommand(Guid TraineeId, string DeletedById) : ICommand;
