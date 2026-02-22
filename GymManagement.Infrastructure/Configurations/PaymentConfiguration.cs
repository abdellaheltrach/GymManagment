using GymManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymManagement.Infrastructure.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.ToTable("Payments");
            builder.HasKey(p => p.Id);

            builder.Property(p => p.RowVersion).IsRowVersion();

            builder.Property(p => p.Amount).HasColumnType("decimal(10,2)");
            builder.Property(p => p.RecordedById).IsRequired().HasMaxLength(450);
            builder.Property(p => p.ReferenceNumber).HasMaxLength(100);
            builder.Property(p => p.Notes).HasMaxLength(500);
            builder.Property(p => p.RefundedById).HasMaxLength(450);
            builder.Property(p => p.RefundReason).HasMaxLength(500);

            // Indexes
            builder.HasIndex(p => p.MembershipId);
            builder.HasIndex(p => p.PaidAt);
        }
    }
}
