using GymManagement.Application.Common.DTOs;
using GymManagement.Domain.Entities;

namespace GymManagement.Application.Common.Mappings;

public partial class MappingProfile
{
    private void ConfigureExerciseMappings()
    {
        CreateMap<Exercise, ExerciseDto>()
            .ConstructUsing((src, ctx) => new ExerciseDto(
                src.Id, src.Name, src.Sets, src.Reps, src.WeightKg,
                src.DurationSeconds, src.RestSeconds, src.Order, src.Notes
            ));
    }
}
