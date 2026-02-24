using AutoMapper;
using GymManagement.Application.Common.DTOs;
using GymManagement.Domain.Entities;

namespace GymManagement.Application.Common.Mappings;

public partial class MappingProfile
{
    private void ConfigureTrainerMappings()
    {
        CreateMap<Trainer, TrainerSummaryDto>()
            .ConstructUsing((src, ctx) => new TrainerSummaryDto(
                src.Id,
                src.FullName,
                src.Email,
                src.Specialization,
                src.TrainerAssignments.Count(a => a.RemovedAt == null),
                src.IsActive
            ));

        CreateMap<Trainer, TrainerDetailDto>()
            .ConstructUsing((src, ctx) => new TrainerDetailDto(
                src.Id,
                src.FirstName,
                src.LastName,
                src.Email,
                src.Phone,
                src.Specialization,
                src.Bio,
                src.YearsOfExperience,
                src.SalaryType,
                src.BaseSalary,
                src.CommissionPerTrainee,
                src.HireDate,
                src.IsActive,
                src.TrainerAssignments.Count(a => a.RemovedAt == null)
            ));
    }
}
