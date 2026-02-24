using GymManagement.Application.Common.Behaviours;
using MediatR;

namespace GymManagement.Application._Features.Trainees.Commands.Models;

/// <summary>
/// GDPR compliance command — blanks all PII fields while retaining
/// attendance and payment records (de-linked from identity).
/// Once anonymised, the operation cannot be reversed.
/// </summary>
public record AnonymiseTraineeCommand(Guid TraineeId, string RequestedById) : ICommand;
