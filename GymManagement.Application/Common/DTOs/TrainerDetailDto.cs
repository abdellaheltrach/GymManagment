using GymManagement.Domain.Enums;

namespace GymManagement.Application.Common.DTOs;

public record TrainerDetailDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    TrainerSpecialization Specialization,
    string? Bio,
    int YearsOfExperience,
    SalaryType SalaryType,
    decimal? BaseSalary,
    decimal? CommissionPerTrainee,
    DateTime HireDate,
    bool IsActive,
    int AssignedTraineeCount
);
