using GymManagement.Application.Authorization;
using GymManagement.Application.Extensions;
using GymManagement.Domain.Entities.Identity;
using GymManagement.Domain.Enums;
using GymManagement.Domain.Interfaces;
using GymManagement.Infrastructure;
using GymManagement.Infrastructure.Context;
using GymManagement.Infrastructure.Middleware;
using GymManagement.Infrastructure.Persistence.Seeders;
using GymManagement.Web.Filters;
using GymManagement.Web.Middleware;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Formatting.Json;
using System.Threading.RateLimiting;

#region Bootstrap Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();
#endregion

try
{
    var builder = WebApplication.CreateBuilder(args);

    #region Configure Serilog
    builder.Host.UseSerilog((ctx, services, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithEnvironmentName()
        .WriteTo.Console(new JsonFormatter())
        .WriteTo.Seq(ctx.Configuration["Seq:Url"] ?? "http://localhost:5341"));
    #endregion

    #region Add MVC Services
    builder.Services.AddControllersWithViews(options =>
    {
        options.Filters.Add(
            new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
    });
    #endregion

    #region Register Application and Infrastructure Services
    builder.Services
        .AddApplicationServicesRegistration()
        .AddInfrastructureServicesRegistration(builder.Configuration);
    #endregion

    #region Configure Authentication Cookie
    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";

        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;

        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.Name = "GymMgmt.Auth";
    });
    #endregion

    #region Configure Authorization Policies
    builder.Services.AddAuthorization(options =>
    {
        options.FallbackPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();

        // Role-only (admin)
        options.AddPolicy("CanManageTrainers", p => p.RequireRole("Admin"));
        options.AddPolicy("CanManagePlans", p => p.RequireRole("Admin"));
        options.AddPolicy("CanManageStaff", p => p.RequireRole("Admin"));
        options.AddPolicy("TrainerAccess", p => p.RequireRole("Admin", "Trainer"));

        // Bitmask policies — Admin bypasses in HasPermissionHandler
        void AddPerm(string name, ReceptionistPermission perm)
            => options.AddPolicy(name, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new HasPermissionRequirement(perm));
            });

        AddPerm("CanManageTrainees", ReceptionistPermission.RegisterTrainees);
        AddPerm("CanEditTraineeProfile", ReceptionistPermission.EditTraineeProfile);

        AddPerm("CanRecordPayments", ReceptionistPermission.RecordPayments);

        AddPerm("CanAssignMemberships", ReceptionistPermission.AssignMemberships);
        AddPerm("CanFreezeMemberships", ReceptionistPermission.FreezeMemberships);
        AddPerm("CanCancelMemberships", ReceptionistPermission.CancelMemberships);
        AddPerm("CanSuspendMemberships", ReceptionistPermission.SuspendMemberships);

        AddPerm("CanCheckIn", ReceptionistPermission.CheckInTrainees);
        AddPerm("CanMarkAttendance", ReceptionistPermission.CheckInTrainees);
        AddPerm("CanViewReports", ReceptionistPermission.ViewFinancialReports);
    });
    #endregion

    #region Configure Rate Limiting
    builder.Services.AddRateLimiter(options =>
    {
        options.AddFixedWindowLimiter("LoginPolicy", o =>
        {
            o.Window = TimeSpan.FromMinutes(1);
            o.PermitLimit = 10;
            o.QueueLimit = 0;
            o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        });

        options.AddSlidingWindowLimiter("GlobalPolicy", o =>
        {
            o.Window = TimeSpan.FromMinutes(1);
            o.PermitLimit = 200;
            o.SegmentsPerWindow = 4;
            o.QueueLimit = 0;
        });

        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    });
    #endregion

    #region Add Health Checks
    builder.Services.AddHealthChecks()
        .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")!)
        .AddDbContextCheck<AppDbContext>()
        .AddHangfire(setup =>
        {
            setup.MinimumAvailableServers = 1;
        });
    #endregion

    #region Add Memory Cache
    builder.Services.AddMemoryCache(); // Required for Hangfire Dashboard to store job information and dashboard state
    #endregion

    #region Add Authorization Handlers
    builder.Services.AddScoped<IAuthorizationHandler, HasPermissionHandler>();
    #endregion


    // builder.Services.AddScoped<DatabaseSeeder>(); // Now registered in Infrastructure


    #region Build Application
    var app = builder.Build();
    #endregion

    #region Apply EF Core Migration & Seeding
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (app.Environment.IsDevelopment())
        {
            await dbContext.Database.MigrateAsync();
        }

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        await RoleSeeder.SeedAsync(roleManager);
        await uow.SaveChangesAsync(); // Save roles before user seeding

        await UserSeeder.SeedAsync(userManager, uow);
        await MembershipPlanSeeder.SeedAsync(uow);
    }
    #endregion

    #region Configure Exception Handling
    if (!app.Environment.IsDevelopment()) // In production, use a custom error page and enable HSTS
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }
    #endregion

    #region Configure Middleware Pipeline

    app.UseHttpsRedirection();

    app.UseStaticFiles();

    app.UseMiddleware<ExceptionHandlingMiddleware>();

    app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");

    app.UseSerilogRequestLogging();

    app.UseRouting();

    app.UseMiddleware<SecurityHeadersMiddleware>();

    app.UseRateLimiter();

    app.UseAuthentication();
    app.UseMiddleware<ReceptionistPermissionRefreshMiddleware>();
    app.UseAuthorization();

    #endregion

    #region Configure Hangfire Dashboard
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = [new HangfireAuthorizationFilter()],
        DashboardTitle = "Gym Management — Jobs"
    });
    #endregion

    #region Register Hangfire Jobs
    InfrastructureServicesRegistration.RegisterHangfireJobs();
    #endregion

    #region Configure Routes
    //{landing} Map to Dashboard controller when when no specific rout in URL
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Dashboard}/{action=Index}/{id?}");
    #endregion

    #region Configure Health Check Endpoints
    // Returns Healthy if {checks}  ASP.NET Core app is running
    app.MapHealthChecks("/health/live", new HealthCheckOptions
    {
        Predicate = _ => false
    }).AllowAnonymous();

    // Returns Healthy if all registered health checks are healthy
    app.MapHealthChecks("/health/ready", new HealthCheckOptions()).AllowAnonymous();

    #endregion

    #region Run Application
    await app.RunAsync();
    #endregion
}
catch (Exception ex) when (ex is not HostAbortedException) // Catch all exceptions except those related to host shutdown
{
    Log.Fatal(ex, "Application terminated unexpectedly"); // Log the exception with Serilog at the Fatal level
}
finally
{
    Log.CloseAndFlush(); // Ensure all logs are flushed before application exits
}
