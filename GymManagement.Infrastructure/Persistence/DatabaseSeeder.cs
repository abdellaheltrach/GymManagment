using GymManagement.Domain.Entities;
using GymManagement.Domain.Entities.Identity;
using GymManagement.Domain.Enums;
using GymManagement.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GymManagement.Infrastructure.Persistence
{
    /// <summary>
    /// Runs once at startup after migrations.
    /// Idempotent — safe to run on every startup, skips if data already exists.
    /// Moved to Infrastructure layer as it handles persistence-specific seeding.
    /// </summary>
    public class DatabaseSeeder(
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager,
        IUnitOfWork uow,
        IConfiguration config,
        ILogger<DatabaseSeeder> logger)
    {
        public async Task SeedAsync()
        {
            await uow.ExecuteInTransactionAsync(async _ =>
            {
                await SeedRolesAsync();
                await SeedAdminUserAsync();
                await SeedMembershipPlansAsync();
            });
        }

        private async Task SeedRolesAsync()
        {
            string[] roles = ["Admin", "Trainer", "Receptionist", "Trainee"];

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                    logger.LogInformation("Added role to tracker: {Role}", role);
                }
            }
        }

        private async Task SeedAdminUserAsync()
        {
            const string adminEmail = "admin@gym.com";

            var admin = await userManager.FindByEmailAsync(adminEmail);

            if (admin is not null)
                return;

            // Password from env in production, fallback for dev only
            var password = config["Seed:AdminPassword"] ?? "Admin@123456!";

            var seedAdmin = new ApplicationUser
            {
                UserName = "ysf",
                Email = adminEmail,
                FirstName = "System",
                LastName = "Admin",
                EmailConfirmed = true,
                IsActive = true
            };

            var result = await userManager.CreateAsync(seedAdmin, password);

            if (!result.Succeeded)
            {
                logger.LogError("Failed to prepare admin user: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
                return;
            }

            await userManager.AddToRoleAsync(seedAdmin, "Admin");
            logger.LogInformation("Prepared admin user with role: {Email}", adminEmail);
        }

        private async Task SeedMembershipPlansAsync()
        {
            var existingCount = await uow.MembershipPlans.CountAsync();
            if (existingCount > 0) return;

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

            logger.LogInformation("Added {Count} membership plans to tracker.", plans.Length);
        }
    }
}
