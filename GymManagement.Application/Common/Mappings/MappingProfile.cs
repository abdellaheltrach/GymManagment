using AutoMapper;

namespace GymManagement.Application.Common.Mappings;

public partial class MappingProfile : Profile
{
    public MappingProfile()
    {
        ConfigureTraineeMappings();
        ConfigureTrainerMappings();
        ConfigureMembershipMappings();
        ConfigureMembershipPlanMappings();
        ConfigurePaymentMappings();
        ConfigureAttendanceMappings();
        ConfigureExerciseMappings();
        ConfigureTrainingProgramMappings();
        ConfigureProgressRecordMappings();
    }
}

