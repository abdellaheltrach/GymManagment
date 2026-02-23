using GymManagement.Domain.Enums;
using GymManagement.Infrastructure.Bases.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace GymManagement.Infrastructure.Jobs
{
    /// <summary>
    /// Hangfire recurring job — runs on the 1st of each month at 02:00.
    /// Pre-computes the previous month's revenue aggregate and stores it in IMemoryCache.
    /// The admin dashboard reads from cache — never re-computes on every page load.
    ///
    /// Cache key format: "revenue:{year}:{month}" e.g. "revenue:2025:3"
    /// Cache expiry: 35 days (covers the full next month before the next run)
    /// </summary>
    public class RevenueReportJob
    {
        private readonly IUnitOfWork _uow;
        private readonly IMemoryCache _cache;
        private readonly ILogger<RevenueReportJob> _logger;

        // Public so the dashboard can read the same key
        public static string CacheKey(int year, int month) => $"revenue:{year}:{month}";

        public RevenueReportJob(
            IUnitOfWork uow,
            IMemoryCache cache,
            ILogger<RevenueReportJob> logger)
        {
            _uow = uow;
            _cache = cache;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            var now = DateTime.UtcNow;
            var previousMonth = now.AddMonths(-1);
            var year = previousMonth.Year;
            var month = previousMonth.Month;
            var monthStart = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            var monthEnd = monthStart.AddMonths(1);

            _logger.LogInformation(
                "RevenueReportJob computing revenue for {Year}-{Month:D2}", year, month);

            // All non-refunded payments within the previous calendar month
            var payments = await _uow.Payments.FindAsync(p =>
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

            var key = CacheKey(year, month);
            _cache.Set(key, aggregate, TimeSpan.FromDays(35));

            _logger.LogInformation(
                "Revenue cached for {Year}-{Month:D2}. Total: {Total:C}",
                year, month, aggregate.TotalRevenue);
        }
    }

    /// <summary>
    /// Serialisable aggregate stored in cache and returned to the dashboard query.
    /// </summary>
    public sealed record MonthlyRevenueAggregate
    {
        public int Year { get; init; }
        public int Month { get; init; }
        public decimal TotalRevenue { get; init; }
        public int PaymentCount { get; init; }
        public decimal CashRevenue { get; init; }
        public decimal CardRevenue { get; init; }
        public decimal TransferRevenue { get; init; }
        public DateTime ComputedAt { get; init; }
    }
}
