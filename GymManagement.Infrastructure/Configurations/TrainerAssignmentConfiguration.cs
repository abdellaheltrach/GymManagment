using GymManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymManagement.Infrastructure.Configurations
{
    public class TrainerAssignmentConfiguration : IEntityTypeConfiguration<TrainerAssignment>
    {
        public void Configure(EntityTypeBuilder<TrainerAssignment> builder)
        {
            builder.ToTable("TrainerAssignments");
            builder.HasKey(ta => ta.Id);

            builder.Property(ta => ta.AssignedById).IsRequired().HasMaxLength(450);
            builder.Property(ta => ta.RemovedById).HasMaxLength(450);
            builder.Property(ta => ta.RemovalReason).HasMaxLength(500);

            builder.Ignore(ta => ta.IsActive);

            // Composite index: find active assignment for a trainee fast
            builder.HasIndex(ta => new { ta.TraineeId, ta.RemovedAt });
            builder.HasIndex(ta => ta.TrainerId);
        }
    }
}
