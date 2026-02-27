using GymManagement.Application.Common.Behaviours;

namespace GymManagement.Application._Features.Trainers.Commands.Models;

public record RemoveTrainerCommand(
    Guid TraineeId,
    string RemovedById,
    string? Reason
) : ICommand;
