using GymManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymManagement.Infrastructure.Configurations
{

    public class FrozenPeriodConfiguration : IEntityTypeConfiguration<FrozenPeriod>
    {
        public void Configure(EntityTypeBuilder<FrozenPeriod> builder)
        {
            builder.ToTable("FrozenPeriods");
            builder.HasKey(f => f.Id);

            builder.Property(f => f.Reason).IsRequired().HasMaxLength(500);
            builder.Property(f => f.RequestedById).IsRequired().HasMaxLength(450);

            builder.Ignore(f => f.DurationDays);

            builder.HasIndex(f => f.MembershipId);
            builder.HasIndex(f => f.FrozenTo); // Hangfire job scans this
        }
    }
}
