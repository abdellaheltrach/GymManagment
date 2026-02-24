using GymManagement.Application.Common.Behaviours;
using GymManagement.Application.Common.DTOs;
using MediatR;

namespace GymManagement.Application._Features.Trainees.Queries.Models;

public record GetTraineeByIdQuery(Guid TraineeId) : IQuery<TraineeDetailDto>;
