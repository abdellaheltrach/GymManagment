using GymManagement.Application.Common.Behaviours;

namespace GymManagement.Application.Features.Trainers.Commands.Models;

public record AssignTrainerCommand(
    Guid TrainerId,
    Guid TraineeId,
    string AssignedById
) : ICommand<Guid>;
