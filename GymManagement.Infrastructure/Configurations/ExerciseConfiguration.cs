using GymManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymManagement.Infrastructure.Configurations
{
    public class ExerciseConfiguration : IEntityTypeConfiguration<Exercise>
    {
        public void Configure(EntityTypeBuilder<Exercise> builder)
        {
            builder.ToTable("Exercises");
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
            builder.Property(e => e.WeightKg).HasColumnType("decimal(6,2)");
            builder.Property(e => e.Notes).HasMaxLength(500);

            builder.HasIndex(e => new { e.TrainingProgramId, e.Order });
        }
    }
}
