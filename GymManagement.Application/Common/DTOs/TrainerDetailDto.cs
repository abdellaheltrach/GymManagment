using GymManagement.Domain.Enums;

namespace GymManagement.Application.Common.DTOs;

public record TrainerDetailDto(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string Phone,
    TrainerSpecialization Specialization,
    string? Bio,
    int YearsOfExperience,
    DateTime HireDate,
    bool IsActive,
    SalaryType SalaryType,
    decimal? BaseSalary,
    decimal? CommissionPerTrainee,
    int AssignedTraineeCount
);
