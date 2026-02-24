using GymManagement.Application.Common.Behaviours;
using MediatR;

namespace GymManagement.Application._Features.Trainers.Commands.Models;

public record AssignTrainerCommand(
    Guid TrainerId,
    Guid TraineeId,
    string AssignedById
) : ICommand<Guid>;
