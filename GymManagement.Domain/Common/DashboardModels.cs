namespace GymManagement.Domain.Common;

/// <summary>
/// Serialisable aggregate stored in cache and returned to the dashboard query.
/// This matches the data pre-computed by the RevenueReportJob.
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

public static class CacheKeys
{
    public static string MonthlyRevenue(int year, int month) => $"revenue:{year}:{month}";
}
