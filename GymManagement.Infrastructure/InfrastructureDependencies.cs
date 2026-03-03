using GymManagement.Domain.Entities.Identity;
using GymManagement.Domain.Options;
using GymManagement.Infrastructure.Bases;
using GymManagement.Domain.Interfaces;
using GymManagement.Infrastructure.Context;
using GymManagement.Infrastructure.Interceptors;
using GymManagement.Infrastructure.Jobs;
using GymManagement.Infrastructure.Persistence;
using GymManagement.Infrastructure.Identity;
using GymManagement.Infrastructure.Persistence.Identity;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GymManagement.Infrastructure
{
    public static class InfrastructureServicesRegistration
    {
        public static IServiceCollection AddInfrastructureServicesRegistration(this IServiceCollection services,
               IConfiguration configuration)
        {
            // ── Options Configuration ──────────────────────────────────────────
            var jwtSettings = new JwtSettings();
            var cookieSettings = new CookieSettings();
            var emailSettings = new EmailSettings();
            
            configuration.GetSection(nameof(JwtSettings)).Bind(jwtSettings);
            configuration.GetSection(nameof(CookieSettings)).Bind(cookieSettings);
            configuration.GetSection(nameof(EmailSettings)).Bind(emailSettings);

            services.AddSingleton(jwtSettings);
            services.AddSingleton(cookieSettings);
            services.AddSingleton(emailSettings);

            services.AddScoped<IEmailService, SmtpEmailService>();
            services.AddMemoryCache();

            // ── Audit interceptor ──────────────────────────────────────────────
            // Registered as Singleton because interceptors are resolved once
            // and IHttpContextAccessor is thread-safe.
            services.AddHttpContextAccessor();
            services.AddSingleton<AuditInterceptor>();
            services.AddSingleton<SoftDeleteInterceptor>();
            services.AddSingleton<TimestampInterceptor>();

            // ── EF Core + SQL Server ───────────────────────────────────────────
            services.AddDbContext<AppDbContext>((sp, options) =>
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sql =>
                    {
                        sql.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(5),
                            errorNumbersToAdd: null);

                        sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                    });

                // Wire in the interceptors in order
                options.AddInterceptors(
                    sp.GetRequiredService<SoftDeleteInterceptor>(),
                    sp.GetRequiredService<TimestampInterceptor>(),
                    sp.GetRequiredService<AuditInterceptor>());

                // In development, log sensitive data and enable detailed errors
                // These are safe because AddDbContext respects IsDevelopment implicitly
                // — switch off via environment in Production
            });

            // ── ASP.NET Core Identity ──────────────────────────────────────────
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Password
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 12;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;

                // Lockout — prevent brute force
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.AllowedForNewUsers = true;

                // User
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false; // Set true once email is configured
            })
            .AddUserStore<NoSaveUserStore>()
            .AddRoleStore<NoSaveRoleStore>()
            .AddDefaultTokenProviders();

            // ── Repository & Unit of Work ──────────────────────────────────────
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // ── Identity Service ──────────────────────────────────────────────
            services.AddScoped<IIdentityService, IdentityService>();

            // ── Memory cache (for RevenueReportJob) ───────────────────────────
            // Already registered above

            // ── Hangfire ───────────────────────────────────────────────────────
            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(
                    configuration.GetConnectionString("DefaultConnection"),
                    new SqlServerStorageOptions
                    {
                        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                        QueuePollInterval = TimeSpan.Zero,
                        UseRecommendedIsolationLevel = true,
                        DisableGlobalLocks = true
                    }));

            services.AddHangfireServer(options =>
            {
                options.WorkerCount = 2; // Low — this is background work, not a web server
            });

            // ── Hangfire job classes (injectable) ─────────────────────────────
            services.AddScoped<MembershipExpiryJob>();
            services.AddScoped<AutoSuspendExpiredJob>();
            services.AddScoped<FreezeEndRecalculationJob>();
            services.AddScoped<RevenueReportJob>();

            return services;
        }

        /// <summary>
        /// Call this after app.UseHangfireDashboard() in Program.cs.
        /// Registers all recurring jobs with their CRON schedules.
        /// </summary>
        public static void RegisterHangfireJobs()
        {
            // Daily at 00:05 — auto-expire memberships
            RecurringJob.AddOrUpdate<AutoSuspendExpiredJob>(
                "auto-suspend-expired",
                job => job.ExecuteAsync(),
                "5 0 * * *",          // CRON: 00:05 every day
                new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

            // Daily at 06:00 — send expiry notifications
            RecurringJob.AddOrUpdate<MembershipExpiryJob>(
                "membership-expiry-check",
                job => job.ExecuteAsync(),
                "0 6 * * *",          // CRON: 06:00 every day
                new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

            // Every hour — unfreeze and recalculate end dates
            RecurringJob.AddOrUpdate<FreezeEndRecalculationJob>(
                "freeze-end-recalculation",
                job => job.ExecuteAsync(),
                Cron.Hourly(),
                new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

            // 1st of every month at 02:00 — pre-compute revenue report
            RecurringJob.AddOrUpdate<RevenueReportJob>(
                "revenue-report-generation",
                job => job.ExecuteAsync(),
                "0 2 1 * *",          // CRON: 02:00 on day 1 of every month
                new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });
        }
    }
}