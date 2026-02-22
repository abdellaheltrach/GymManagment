using GymManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymManagement.Infrastructure.Configurations
{

    public class TrainerConfiguration : IEntityTypeConfiguration<Trainer>
    {
        public void Configure(EntityTypeBuilder<Trainer> builder)
        {
            builder.ToTable("Trainers");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.RowVersion).IsRowVersion();

            builder.Property(t => t.ApplicationUserId).IsRequired().HasMaxLength(450);
            builder.Property(t => t.FirstName).IsRequired().HasMaxLength(100);
            builder.Property(t => t.LastName).IsRequired().HasMaxLength(100);
            builder.Property(t => t.Email).IsRequired().HasMaxLength(256);
            builder.Property(t => t.Phone).IsRequired().HasMaxLength(20);
            builder.Property(t => t.Bio).HasMaxLength(1000);
            builder.Property(t => t.PhotoPath).HasMaxLength(500);
            builder.Property(t => t.BaseSalary).HasColumnType("decimal(10,2)");
            builder.Property(t => t.CommissionPerTrainee).HasColumnType("decimal(10,2)");

            builder.Ignore(t => t.FullName);

            // Indexes
            builder.HasIndex(t => t.Email).IsUnique();
            builder.HasIndex(t => t.ApplicationUserId).IsUnique();
            builder.HasIndex(t => t.Specialization);
            builder.HasIndex(t => t.IsDeleted);

            builder.HasQueryFilter(t => !t.IsDeleted);

            // Relationships
            builder.HasMany(t => t.TrainerAssignments)
                   .WithOne(ta => ta.Trainer)
                   .HasForeignKey(ta => ta.TrainerId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(t => t.TrainingPrograms)
                   .WithOne(tp => tp.Trainer)
                   .HasForeignKey(tp => tp.TrainerId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}