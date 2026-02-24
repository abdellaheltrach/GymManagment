using GymManagement.Application.Common.Behaviours;
using MediatR;

namespace GymManagement.Application._Features.Trainers.Commands.Models;

public record RemoveTrainerCommand(
    Guid TraineeId,
    string RemovedById,
    string? Reason
) : ICommand;
