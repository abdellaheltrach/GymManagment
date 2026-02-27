using GymManagement.Application.Common.DTOs;
using GymManagement.Domain.Entities;

namespace GymManagement.Application.Common.Mappings;

public partial class MappingProfile
{
    private void ConfigureMembershipPlanMappings()
    {
        CreateMap<MembershipPlan, MembershipPlanDto>()
            .ConstructUsing((src, ctx) => new MembershipPlanDto(
                src.Id,
                src.Name,
                src.Description,
                src.DurationDays,
                src.Price,
                src.AccessLevel,
                src.IncludesPersonalTrainer,
                src.MaxFreezeDays,
                src.IsActive
            ));
    }
}
