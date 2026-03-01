using GymManagement.Application.Common.Behaviours;
using GymManagement.Domain.Enums;

namespace GymManagement.Application.Features.Trainees.Commands.Models;

public record RegisterTraineeCommand(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string NationalId,
    DateTime DateOfBirth,
    Gender Gender,
    string EmergencyContactName,
    string EmergencyContactPhone,
    string? EmergencyContactRelation,
    string? Address,
    string? MedicalNotes,
    decimal? HeightCm,
    decimal? WeightKg,
    string? CreatedById
) : ICommand<Guid>;
