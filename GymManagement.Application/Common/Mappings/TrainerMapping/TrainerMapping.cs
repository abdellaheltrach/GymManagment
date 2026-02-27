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
                src.FirstName,
                src.LastName,
                src.FullName,
                src.Email,
                src.Phone,
                src.Specialization,
                src.YearsOfExperience,
                src.TrainerAssignments.Count(a => a.RemovedAt == null),
                src.IsActive
            ));

        CreateMap<Trainer, TrainerDetailDto>()
            .ConstructUsing((src, ctx) => new TrainerDetailDto(
                src.Id,
                src.FirstName,
                src.LastName,
                src.FullName,
                src.Email,
                src.Phone,
                src.Specialization,
                src.Bio,
                src.YearsOfExperience,
                src.HireDate,
                src.IsActive,
                src.SalaryType,
                src.BaseSalary,
                src.CommissionPerTrainee,
                src.TrainerAssignments.Count(a => a.RemovedAt == null)
            ));
    }
}
