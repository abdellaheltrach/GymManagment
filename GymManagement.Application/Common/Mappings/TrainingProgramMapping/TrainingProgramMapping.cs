using GymManagement.Application.Common.DTOs;
using GymManagement.Domain.Entities;

namespace GymManagement.Application.Common.Mappings;

public partial class MappingProfile
{
    private void ConfigureTrainingProgramMappings()
    {
        CreateMap<TrainingProgram, TrainingProgramDto>()
            .ConstructUsing((src, ctx) => new TrainingProgramDto(
                src.Id,
                src.TrainerId,
                src.Trainer != null ? src.Trainer.FullName : string.Empty,
                src.TraineeId,
                src.Trainee != null ? src.Trainee.FullName : string.Empty,
                src.Title,
                src.Description,
                src.StartDate,
                src.EndDate,
                src.IsActive,
                src.Exercises
                    .OrderBy(e => e.Order)
                    .Select(e => ctx.Mapper.Map<ExerciseDto>(e))
                    .ToList()
            ));
    }
}
