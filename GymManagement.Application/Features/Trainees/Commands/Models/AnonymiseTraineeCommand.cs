using GymManagement.Application.Common.Behaviours;

namespace GymManagement.Application._Features.Trainees.Commands.Models;


public record AnonymiseTraineeCommand(Guid TraineeId, string RequestedById) : ICommand;
