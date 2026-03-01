using GymManagement.Application.Common.Behaviours;

namespace GymManagement.Application.Features.Trainees.Commands.Models;

public record DeleteTraineeCommand(Guid TraineeId, string DeletedById) : ICommand;
