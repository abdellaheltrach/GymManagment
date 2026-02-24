using GymManagement.Domain.Enums;
using GymManagement.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace GymManagement.Infrastructure.Jobs
{
    /// <summary>
    /// Hangfire recurring job — runs daily at 00:05.
    /// Finds all Active memberships whose EndDate has passed and sets Status = Expired.
    /// Running at 00:05 (not 00:00) avoids race conditions with midnight renewals.
    /// </summary>
    public class AutoSuspendExpiredJob
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<AutoSuspendExpiredJob> _logger;

        public AutoSuspendExpiredJob(IUnitOfWork uow, ILogger<AutoSuspendExpiredJob> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            var now = DateTime.UtcNow;
            _logger.LogInformation("AutoSuspendExpiredJob started at {Time}", now);

            var expired = await _uow.Memberships.FindAsync(m =>
                m.Status == MembershipStatus.Active &&
                m.EndDate < now);

            if (!expired.Any())
            {
                _logger.LogInformation("No memberships to expire.");
                return;
            }

            foreach (var membership in expired)
            {
                membership.Status = MembershipStatus.Expired;
                membership.UpdatedAt = now;
                _uow.Memberships.Update(membership);
            }

            await _uow.SaveChangesAsync();
            _logger.LogInformation("Expired {Count} memberships.", expired.Count);
        }
    }
}
