using GymManagement.Domain.Common;
using GymManagement.Domain.Enums;
using GymManagement.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace GymManagement.Infrastructure.Jobs
{
    /// <summary>
    /// Hangfire recurring job — runs on the 1st of each month at 02:00.
    /// Pre-computes the previous month's revenue aggregate and stores it in IMemoryCache.
    /// The admin dashboard reads from cache — never re-computes on every page load.
    /// </summary>
    public class RevenueReportJob(
        IUnitOfWork uow,
        IMemoryCache cache,
        ILogger<RevenueReportJob> logger)
    {
        public async Task ExecuteAsync()
        {
            var now = DateTime.UtcNow;
            var previousMonth = now.AddMonths(-1);
            var year = previousMonth.Year;
            var month = previousMonth.Month;
            var monthStart = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            var monthEnd = monthStart.AddMonths(1);

            logger.LogInformation(
                "RevenueReportJob computing revenue for {Year}-{Month:D2}", year, month);

            // All non-refunded payments within the previous calendar month
            var payments = await uow.Payments.FindAsync(p =>
                p.PaidAt >= monthStart &&
                p.PaidAt < monthEnd &&
                !p.IsRefunded);

            var aggregate = new MonthlyRevenueAggregate
            {
                Year = year,
                Month = month,
                TotalRevenue = payments.Sum(p => p.Amount),
                PaymentCount = payments.Count,
                CashRevenue = payments.Where(p => p.Method == PaymentMethod.Cash).Sum(p => p.Amount),
                CardRevenue = payments.Where(p => p.Method == PaymentMethod.Card).Sum(p => p.Amount),
                TransferRevenue = payments.Where(p => p.Method == PaymentMethod.BankTransfer).Sum(p => p.Amount),
                ComputedAt = now
            };

            var key = CacheKeys.MonthlyRevenue(year, month);
            cache.Set(key, aggregate, TimeSpan.FromDays(35));

            logger.LogInformation(
                "Revenue cached for {Year}-{Month:D2}. Total: {Total:C}",
                year, month, aggregate.TotalRevenue);
        }
    }
}
