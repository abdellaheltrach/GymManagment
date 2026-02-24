using GymManagement.Domain.Enums;
using GymManagement.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace GymManagement.Infrastructure.Jobs
{
    /// <summary>
    /// Hangfire recurring job — runs every hour.
    /// Finds FrozenPeriods whose FrozenTo has passed and the membership is still Frozen.
    /// For each:
    ///   1. Calculates days frozen
    ///   2. Extends Membership.EndDate by that many days
    ///   3. Updates Membership.TotalFrozenDays
    ///   4. Sets Membership.Status = Active
    ///
    /// Running hourly (not daily) ensures members are unfrozen within the hour
    /// their freeze period ends — not the next day.
    /// </summary>
    public class FreezeEndRecalculationJob
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<FreezeEndRecalculationJob> _logger;

        public FreezeEndRecalculationJob(IUnitOfWork uow, ILogger<FreezeEndRecalculationJob> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            var now = DateTime.UtcNow;
            _logger.LogInformation("FreezeEndRecalculationJob started at {Time}", now);

            // Find freeze periods that have ended but the membership is still marked Frozen
            var endedFreezes = await _uow.FrozenPeriods.FindAsync(f =>
                f.FrozenTo < now);

            if (!endedFreezes.Any())
            {
                _logger.LogInformation("No ended freeze periods found.");
                return;
            }

            int processed = 0;

            foreach (var freeze in endedFreezes)
            {
                var membership = await _uow.Memberships.GetByIdAsync(freeze.MembershipId);

                if (membership is null || membership.Status != MembershipStatus.Frozen)
                    continue;

                var frozenDays = (freeze.FrozenTo - freeze.FrozenFrom).Days;

                // Extend the end date by the frozen duration
                membership.EndDate = membership.EndDate.AddDays(frozenDays);
                membership.TotalFrozenDays += frozenDays;
                membership.Status = MembershipStatus.Active;
                membership.UpdatedAt = now;

                _uow.Memberships.Update(membership);
                processed++;

                _logger.LogInformation(
                    "Unfroze membership {MembershipId}. Extended EndDate by {Days} days to {NewEnd}.",
                    membership.Id, frozenDays, membership.EndDate);
            }

            if (processed > 0)
                await _uow.SaveChangesAsync();

            _logger.LogInformation("FreezeEndRecalculationJob processed {Count} memberships.", processed);
        }
    }
}
