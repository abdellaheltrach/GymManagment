using GymManagement.Application.Common.DTOs;
using GymManagement.Domain.Entities;

namespace GymManagement.Application.Common.Mappings;

public partial class MappingProfile
{
    private void ConfigureAttendanceMappings()
    {
        CreateMap<Attendance, AttendanceDto>()
            .ConstructUsing((src, ctx) => new AttendanceDto(
                src.Id,
                src.TraineeId,
                src.Trainee != null ? src.Trainee.FullName : string.Empty,
                src.CheckInTime,
                src.CheckOutTime,
                src.Duration,
                src.Method
            ));
    }
}
