using GymManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymManagement.Infrastructure.Configurations
{

    public class TraineeConfiguration : IEntityTypeConfiguration<Trainee>
    {
        public void Configure(EntityTypeBuilder<Trainee> builder)
        {
            builder.ToTable("Trainees");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.RowVersion).IsRowVersion();

            builder.Property(t => t.FirstName).IsRequired().HasMaxLength(100);
            builder.Property(t => t.LastName).IsRequired().HasMaxLength(100);
            builder.Property(t => t.Email).IsRequired().HasMaxLength(256);
            builder.Property(t => t.Phone).IsRequired().HasMaxLength(20);
            builder.Property(t => t.NationalId).IsRequired().HasMaxLength(50);
            builder.Property(t => t.EmergencyContactName).IsRequired().HasMaxLength(100);
            builder.Property(t => t.EmergencyContactPhone).IsRequired().HasMaxLength(20);
            builder.Property(t => t.EmergencyContactRelation).HasMaxLength(50);
            builder.Property(t => t.Address).HasMaxLength(500);
            builder.Property(t => t.PhotoPath).HasMaxLength(500);
            builder.Property(t => t.MedicalNotes).HasMaxLength(2000);
            builder.Property(t => t.HeightCm).HasColumnType("decimal(5,2)");
            builder.Property(t => t.WeightKg).HasColumnType("decimal(5,2)");

            // Computed / ignored
            builder.Ignore(t => t.FullName);
            builder.Ignore(t => t.Age);
            builder.Ignore(t => t.Bmi);
            builder.Ignore(t => t.BmiCategory);

            // Indexes
            builder.HasIndex(t => t.Email).IsUnique();
            builder.HasIndex(t => t.NationalId).IsUnique();
            builder.HasIndex(t => t.IsDeleted);

            // Global query filter — applied in DbContext.OnModelCreating
            builder.HasQueryFilter(t => !t.IsDeleted);

            // Relationships
            builder.HasMany(t => t.Memberships)
                   .WithOne(m => m.Trainee)
                   .HasForeignKey(m => m.TraineeId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(t => t.Attendances)
                   .WithOne(a => a.Trainee)
                   .HasForeignKey(a => a.TraineeId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(t => t.ProgressRecords)
                   .WithOne(p => p.Trainee)
                   .HasForeignKey(p => p.TraineeId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(t => t.TrainerAssignments)
                   .WithOne(ta => ta.Trainee)
                   .HasForeignKey(ta => ta.TraineeId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(t => t.TrainingPrograms)
                   .WithOne(tp => tp.Trainee)
                   .HasForeignKey(tp => tp.TraineeId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
