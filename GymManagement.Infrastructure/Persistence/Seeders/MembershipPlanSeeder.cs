using GymManagement.Domain.Entities;
using GymManagement.Domain.Enums;
using GymManagement.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Infrastructure.Persistence.Seeders
{
    public static class MembershipPlanSeeder
    {
        public static async Task SeedAsync(IUnitOfWork uow)
        {
            if (await uow.MembershipPlans.CountAsync() > 0) return;

            var plans = new[]
            {
                new MembershipPlan
                {
                    Name                    = "Monthly",
                    Description             = "Full gym access for 30 days.",
                    DurationDays            = 30,
                    Price                   = 150,
                    AccessLevel             = AccessLevel.Standard,
                    IncludesPersonalTrainer = false,
                    MaxFreezeDays           = 5,
                    IsActive                = true
                },
                new MembershipPlan
                {
                    Name                    = "Quarterly",
                    Description             = "Full gym access for 90 days. Best value.",
                    DurationDays            = 90,
                    Price                   = 400,
                    AccessLevel             = AccessLevel.Standard,
                    IncludesPersonalTrainer = false,
                    MaxFreezeDays           = 10,
                    IsActive                = true
                },
                new MembershipPlan
                {
                    Name                    = "Annual",
                    Description             = "Full gym access for 365 days with premium benefits.",
                    DurationDays            = 365,
                    Price                   = 1400,
                    AccessLevel             = AccessLevel.Premium,
                    IncludesPersonalTrainer = false,
                    MaxFreezeDays           = 30,
                    IsActive                = true
                },
                new MembershipPlan
                {
                    Name                    = "Personal Training",
                    Description             = "30 days with dedicated personal trainer included.",
                    DurationDays            = 30,
                    Price                   = 300,
                    AccessLevel             = AccessLevel.VIP,
                    IncludesPersonalTrainer = true,
                    MaxFreezeDays           = 5,
                    IsActive                = true
                }
            };

            foreach (var plan in plans)
                await uow.MembershipPlans.AddAsync(plan);

            await uow.SaveChangesAsync();
        }
    }
}
