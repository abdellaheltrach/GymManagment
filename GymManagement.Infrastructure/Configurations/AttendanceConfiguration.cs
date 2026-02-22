using GymManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymManagement.Infrastructure.Configurations
{

    public class AttendanceConfiguration : IEntityTypeConfiguration<Attendance>
    {
        public void Configure(EntityTypeBuilder<Attendance> builder)
        {
            builder.ToTable("Attendances");
            builder.HasKey(a => a.Id);

            builder.Property(a => a.RecordedById).IsRequired().HasMaxLength(450);
            builder.Property(a => a.Notes).HasMaxLength(500);

            builder.Ignore(a => a.Duration);

            // Composite index: attendance history by trainee + date
            builder.HasIndex(a => new { a.TraineeId, a.CheckInTime });
            builder.HasIndex(a => a.CheckInTime); // daily stats queries
        }
    }
}
