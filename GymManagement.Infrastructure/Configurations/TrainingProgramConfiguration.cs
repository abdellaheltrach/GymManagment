using GymManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymManagement.Infrastructure.Configurations
{

    public class TrainingProgramConfiguration : IEntityTypeConfiguration<TrainingProgram>
    {
        public void Configure(EntityTypeBuilder<TrainingProgram> builder)
        {
            builder.ToTable("TrainingPrograms");
            builder.HasKey(tp => tp.Id);

            builder.Property(tp => tp.Title).IsRequired().HasMaxLength(200);
            builder.Property(tp => tp.Description).HasMaxLength(1000);

            builder.HasIndex(tp => tp.TraineeId);
            builder.HasIndex(tp => tp.TrainerId);
            builder.HasIndex(tp => tp.IsDeleted);

            builder.HasQueryFilter(tp => !tp.IsDeleted);

            builder.HasMany(tp => tp.Exercises)
                   .WithOne(e => e.TrainingProgram)
                   .HasForeignKey(e => e.TrainingProgramId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
