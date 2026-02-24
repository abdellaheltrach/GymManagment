using GymManagement.Application.Common.Behaviours;
using MediatR;

namespace GymManagement.Application._Features.Trainees.Commands.Models;

public record DeleteTraineeCommand(Guid TraineeId, string DeletedById) : ICommand;
