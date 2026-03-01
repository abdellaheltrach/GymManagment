using GymManagement.Application.Features.Dashboard.Queries.Models;
using GymManagement.Application.Common.DTOs;
using GymManagement.Domain.Common;
using GymManagement.Domain.Enums;
using GymManagement.Domain.Interfaces;
using GymManagement.Domain.Results;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace GymManagement.Application.Features.Dashboard.Queries.Handlers;

public class GetAdminDashboardQueryHandler(IUnitOfWork uow, IMemoryCache cache)
    : IRequestHandler<GetAdminDashboardQuery, Result<AdminDashboardDto>>
{
    public async Task<Result<AdminDashboardDto>> Handle(
        GetAdminDashboardQuery Query, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var weekFromNow = now.AddDays(7);

        // Active members
        var activeMembers = await uow.Memberships.CountAsync(
            m => m.Status == MembershipStatus.Active && m.EndDate > now, ct);

        // New this month
        var newThisMonth = await uow.Trainees.CountAsync(
            t => t.JoinDate >= monthStart, ct);

        // Monthly revenue — from cache if available, live query as fallback
        decimal monthlyRevenue;
        var cacheKey = CacheKeys.MonthlyRevenue(now.Year, now.Month);
        if (cache.TryGetValue(cacheKey, out MonthlyRevenueAggregate? cached) && cached is not null)
        {
            monthlyRevenue = cached.TotalRevenue;
        }
        else
        {
            var payments = await uow.Payments.FindAsync(
                p => p.PaidAt >= monthStart && !p.IsRefunded, ct);
            monthlyRevenue = payments.Sum(p => p.Amount);
        }

        // Expiring this week
        var expiringThisWeek = await uow.Memberships.CountAsync(
            m => m.Status == MembershipStatus.Active &&
                 m.EndDate >= now &&
                 m.EndDate <= weekFromNow, ct);

        // Pending payments
        var pendingPayments = await uow.Memberships.CountAsync(
            m => m.Status == MembershipStatus.PendingPayment, ct);

        // Top 5 trainers by assigned trainee count
        var assignments = await uow.TrainerAssignments.FindAsync(
            a => a.RemovedAt == null, ct);

        var trainerIds = assignments
            .GroupBy(a => a.TrainerId)
            .OrderByDescending(g => g.Count())
            .Take(5)
            .Select(g => g.Key)
            .ToList();

        var topTrainers = new List<TrainerSummaryDto>();
        foreach (var trainerId in trainerIds)
        {
            var trainer = await uow.Trainers.GetByIdAsync(trainerId, ct);
            if (trainer is null) continue;

            var count = assignments.Count(a => a.TrainerId == trainerId);
            topTrainers.Add(new TrainerSummaryDto(
                trainer.Id,
                trainer.FirstName,
                trainer.LastName,
                trainer.FullName,
                trainer.Email,
                trainer.Phone,
                trainer.Specialization,
                trainer.YearsOfExperience,
                count,
                trainer.IsActive
            ));
        }

        var dto = new AdminDashboardDto(
            activeMembers,
            newThisMonth,
            monthlyRevenue,
            expiringThisWeek,
            pendingPayments,
            topTrainers
        );

        return Result<AdminDashboardDto>.Success(dto);
    }
}
