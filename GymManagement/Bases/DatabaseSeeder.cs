using GymManagement.Domain.Entities;
using GymManagement.Domain.Entities.Identity;
using GymManagement.Domain.Enums;
using GymManagement.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace GymManagement.Web.Bases
{
    /// <summary>
    /// Runs once at startup after migrations.
    /// Idempotent — safe to run on every startup, skips if data already exists.
    /// </summary>
    public class DatabaseSeeder
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _uow;
        private readonly IConfiguration _config;
        private readonly ILogger<DatabaseSeeder> _logger;

        public DatabaseSeeder(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            IUnitOfWork uow,
            IConfiguration config,
            ILogger<DatabaseSeeder> logger)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _uow = uow;
            _config = config;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            await SeedRolesAsync();
            await SeedAdminUserAsync();
            await SeedMembershipPlansAsync();
        }

        private async Task SeedRolesAsync()
        {
            string[] roles = ["Admin", "Trainer", "Receptionist", "Trainee"];

            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                    _logger.LogInformation("Seeded role: {Role}", role);
                }
            }
        }

        private async Task SeedAdminUserAsync()
        {
            const string adminEmail = "admin@gym.com";

            var admin = await _userManager.FindByEmailAsync(adminEmail);

            if (admin is not null)
                return;

            // Password from env in production, fallback for dev only
            var password = _config["Seed:AdminPassword"] ?? "Admin@123456!";

            var seedAdmin = new ApplicationUser
            {
                UserName = "ysf",
                Email = adminEmail,
                FirstName = "System",
                LastName = "Admin",
                EmailConfirmed = true,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(seedAdmin, password);
            if (!result.Succeeded)
            {
                _logger.LogError("Failed to create admin user: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
                return;
            }

            await _userManager.AddToRoleAsync(seedAdmin, "Admin");
            _logger.LogInformation("Seeded admin user: {Email}", adminEmail);
        }

        private async Task SeedMembershipPlansAsync()
        {
            var existingCount = await _uow.MembershipPlans.CountAsync();
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
                await _uow.MembershipPlans.AddAsync(plan);

            await _uow.SaveChangesAsync();
            _logger.LogInformation("Seeded {Count} membership plans.", plans.Length);
        }
    }
}
