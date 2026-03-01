using GymManagement.Application.Common.Behaviours;

namespace GymManagement.Application.Features.Trainees.Commands.Models;

public record UpdateTraineeCommand(
    Guid TraineeId,
    string FirstName,
    string LastName,
    string Phone,
    string? Address,
    string? MedicalNotes,
    decimal? HeightCm,
    decimal? WeightKg,
    string EmergencyContactName,
    string EmergencyContactPhone,
    string? EmergencyContactRelation,
    string? UpdatedById
) : ICommand<Guid>;
