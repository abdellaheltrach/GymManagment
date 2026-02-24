using GymManagement.Domain.Enums;

namespace GymManagement.Application.Common.DTOs;

public record TraineeDetailDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string? Address,
    DateTime DateOfBirth,
    int Age,
    Gender Gender,
    string? PhotoPath,
    string? MedicalNotes,
    decimal? HeightCm,
    decimal? WeightKg,
    decimal? Bmi,
    string? BmiCategory,
    DateTime JoinDate,
    MembershipDto? ActiveMembership,
    string? AssignedTrainerName,
    DateTime? LastCheckIn
);
