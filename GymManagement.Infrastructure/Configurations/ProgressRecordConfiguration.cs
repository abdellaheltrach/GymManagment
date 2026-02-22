using GymManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymManagement.Infrastructure.Configurations
{
    public class ProgressRecordConfiguration : IEntityTypeConfiguration<ProgressRecord>
    {
        public void Configure(EntityTypeBuilder<ProgressRecord> builder)
        {
            builder.ToTable("ProgressRecords");
            builder.HasKey(p => p.Id);

            builder.Property(p => p.RecordedById).IsRequired().HasMaxLength(450);
            builder.Property(p => p.WeightKg).HasColumnType("decimal(5,2)");
            builder.Property(p => p.BodyFatPercent).HasColumnType("decimal(4,2)");
            builder.Property(p => p.MuscleMassKg).HasColumnType("decimal(5,2)");
            builder.Property(p => p.ChestCm).HasColumnType("decimal(5,2)");
            builder.Property(p => p.WaistCm).HasColumnType("decimal(5,2)");
            builder.Property(p => p.HipsCm).HasColumnType("decimal(5,2)");
            builder.Property(p => p.ArmCm).HasColumnType("decimal(5,2)");
            builder.Property(p => p.Notes).HasMaxLength(1000);

            // Composite: trainee progress over time — ordered by date
            builder.HasIndex(p => new { p.TraineeId, p.RecordedAt });
        }
    }
}
