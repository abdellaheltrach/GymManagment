using GymManagement.Domain.Enums;
using GymManagement.Domain.Results;
using MediatR;

namespace GymManagement.Application.Features.Trainers.Commands.Models
{
    public record RegisterTrainerCommand(
        string FirstName,
        string LastName,
        string Email,
        string Phone,
        DateTime DateOfBirth,
        TrainerSpecialization Specialization,
        string? Bio,
        int YearsOfExperience,
        SalaryType SalaryType,
        decimal? BaseSalary,
        decimal? CommissionPerTrainee,
        string Password,
        string? CreatedById = null) : IRequest<Result<Guid>>;
}
