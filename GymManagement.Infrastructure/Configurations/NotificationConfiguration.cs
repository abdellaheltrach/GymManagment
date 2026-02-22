using GymManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymManagement.Infrastructure.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("Notifications");
            builder.HasKey(n => n.Id);

            builder.Property(n => n.UserId).IsRequired().HasMaxLength(450);
            builder.Property(n => n.Title).IsRequired().HasMaxLength(200);
            builder.Property(n => n.Body).IsRequired().HasMaxLength(1000);

            builder.HasIndex(n => new { n.UserId, n.IsRead });     // unread count query
            builder.HasIndex(n => n.ScheduledFor);                  // scheduler scan
            builder.HasIndex(n => n.IsSent);
        }
    }
}
