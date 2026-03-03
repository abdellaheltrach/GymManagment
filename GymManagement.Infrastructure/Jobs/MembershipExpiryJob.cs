using GymManagement.Domain.Entities;
using GymManagement.Domain.Enums;
using GymManagement.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace GymManagement.Infrastructure.Jobs
{
    ///<summary>
    /// Hangfire recurring job — runs daily at 06:00.
    /// Finds memberships expiring in 3 days or tomorrow and creates Notification records.
    /// Notifications are then picked up by the email dispatch service.
    /// </summary>
    public class MembershipExpiryJob
    {
        private readonly IUnitOfWork _uow;
        private readonly IEmailService _emailService;
        private readonly ILogger<MembershipExpiryJob> _logger;

        public MembershipExpiryJob(IUnitOfWork uow, IEmailService emailService, ILogger<MembershipExpiryJob> logger)
        {
            _uow = uow;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            var now = DateTime.UtcNow.Date;
            var in1Day = now.AddDays(1);
            var in3Days = now.AddDays(3);

            _logger.LogInformation("MembershipExpiryJob started at {Time}", DateTime.UtcNow);

            // Memberships expiring in exactly 1 or 3 days that are still Active
            var expiring = await _uow.Memberships.FindAsync(m =>
                m.Status == MembershipStatus.Active &&
                (m.EndDate.Date == in1Day || m.EndDate.Date == in3Days));

            if (!expiring.Any())
            {
                _logger.LogInformation("No expiring memberships found.");
                return;
            }

            var notifications = new List<Notification>();

            foreach (var membership in expiring)
            {
                var daysLeft = (membership.EndDate.Date - now).Days;
                var trainee = await _uow.Trainees.GetByIdAsync(membership.TraineeId);
                if (trainee is null) continue;

                // Avoid duplicate notifications — skip if one was already sent today
                var alreadySent = await _uow.Notifications.AnyAsync(n =>
                    n.UserId == trainee.ApplicationUserId &&
                    n.Type == NotificationType.MembershipExpirySoon &&
                    n.CreatedAt.Date == now);

                if (alreadySent) continue;

                notifications.Add(new Notification
                {
                    UserId = trainee.ApplicationUserId ?? string.Empty,
                    Title = "Membership Expiring Soon",
                    Body = $"Your membership expires in {daysLeft} day(s) on " +
                             $"{membership.EndDate:dd MMM yyyy}. Renew now to avoid interruption.",
                    Type = NotificationType.MembershipExpirySoon,
                });

                // Send Email Notification
                if (!string.IsNullOrEmpty(trainee.Email))
                {
                    await _emailService.SendAsync(
                        to: trainee.Email,
                        subject: "Your membership is expiring soon",
                        body: $"Your membership expires in {daysLeft} day(s) on {membership.EndDate:dd MMM yyyy}."
                    );
                }
            }

            if (notifications.Count > 0)
            {
                foreach (var n in notifications)
                    await _uow.Notifications.AddAsync(n);

                await _uow.SaveChangesAsync();
                _logger.LogInformation("Created {Count} expiry notifications.", notifications.Count);
            }
        }
    }

}
