using GymManagement.Application.Common.DTOs;
using GymManagement.Domain.Entities;

namespace GymManagement.Application.Common.Mappings;

public partial class MappingProfile
{
    private void ConfigureMembershipMappings()
    {
        CreateMap<Membership, MembershipDto>()
            .ConstructUsing((src, ctx) => new MembershipDto(
                src.Id,
                src.TraineeId,
                src.Plan != null ? src.Plan.Name : string.Empty,
                src.Plan != null ? src.Plan.AccessLevel : default,
                src.StartDate,
                src.EndDate,
                src.Status,
                src.TotalAmount,
                src.AmountPaid,
                src.RemainingBalance,
                src.IsFullyPaid,
                src.TotalFrozenDays,
                src.Notes,
                src.Plan != null && src.Plan.IncludesPersonalTrainer,
                src.Plan != null ? src.Plan.TrainerAddonFee : 0,
                src.TrainerAddonPaid,
                src.TrainerAddonPaidAt
            ));
    }
}
