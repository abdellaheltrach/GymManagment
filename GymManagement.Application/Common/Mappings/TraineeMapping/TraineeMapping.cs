using AutoMapper;
using GymManagement.Application.Common.DTOs;
using GymManagement.Domain.Entities;

namespace GymManagement.Application.Common.Mappings;

public partial class MappingProfile
{
    private void ConfigureTraineeMappings()
    {
        CreateMap<Trainee, TraineeSummaryDto>()
            .ConstructUsing((src, ctx) => new TraineeSummaryDto(
                src.Id,
                src.FullName,
                src.Email,
                src.Phone,
                src.JoinDate,
                src.Memberships
                    .FirstOrDefault(m => m.Status == Domain.Enums.MembershipStatus.Active)
                    ?.Status,
                src.Memberships
                    .FirstOrDefault(m => m.Status == Domain.Enums.MembershipStatus.Active)
                    ?.EndDate
            ));

        CreateMap<Trainee, TraineeDetailDto>()
            .ConstructUsing((src, ctx) => new TraineeDetailDto(
                src.Id,
                src.FirstName,
                src.LastName,
                src.Email,
                src.Phone,
                src.Address,
                src.DateOfBirth,
                src.Age,
                src.Gender,
                src.PhotoPath,
                src.MedicalNotes,
                src.HeightCm,
                src.WeightKg,
                src.Bmi,
                src.BmiCategory,
                src.JoinDate,
                null,   // ActiveMembership — populated manually in handler
                null,   // AssignedTrainerName — populated manually in handler
                null    // LastCheckIn — populated manually in handler
            ));
    }
}
