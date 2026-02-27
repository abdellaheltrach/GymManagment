using GymManagement.Application.Common.DTOs;
using GymManagement.Domain.Entities;

namespace GymManagement.Application.Common.Mappings;

public partial class MappingProfile
{
    private void ConfigureProgressRecordMappings()
    {
        CreateMap<ProgressRecord, ProgressRecordDto>()
            .ConstructUsing((src, ctx) => new ProgressRecordDto(
                src.Id, src.RecordedAt, src.WeightKg, src.BodyFatPercent,
                src.MuscleMassKg, src.WaistCm, src.ChestCm, src.Notes
            ));
    }
}
