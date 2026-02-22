using GymManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymManagement.Infrastructure.Configurations
{
    public class MembershipConfiguration : IEntityTypeConfiguration<Membership>
    {
        public void Configure(EntityTypeBuilder<Membership> builder)
        {
            builder.ToTable("Memberships");
            builder.HasKey(m => m.Id);

            builder.Property(m => m.RowVersion).IsRowVersion();

            builder.Property(m => m.TotalAmount).HasColumnType("decimal(10,2)");
            builder.Property(m => m.AmountPaid).HasColumnType("decimal(10,2)");
            builder.Property(m => m.Notes).HasMaxLength(1000);

            // Computed — not mapped to columns
            builder.Ignore(m => m.RemainingBalance);
            builder.Ignore(m => m.IsFullyPaid);

            // Indexes — critical for performance
            builder.HasIndex(m => new { m.TraineeId, m.Status });   // active membership lookup
            builder.HasIndex(m => m.EndDate);                        // expiry job scan
            builder.HasIndex(m => m.IsDeleted);

            builder.HasQueryFilter(m => !m.IsDeleted);

            builder.HasMany(m => m.Payments)
                   .WithOne(p => p.Membership)
                   .HasForeignKey(p => p.MembershipId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(m => m.FrozenPeriods)
                   .WithOne(f => f.Membership)
                   .HasForeignKey(f => f.MembershipId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
