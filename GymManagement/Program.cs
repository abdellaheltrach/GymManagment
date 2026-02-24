using GymManagement.Application.Extensions;
using GymManagement.Infrastructure;
using GymManagement.Infrastructure.Context;
using Hangfire;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

#region  add controllers with views and static assets
// Add services to the container.
builder.Services.AddControllersWithViews();
#endregion

#region Services Registration
builder.Services
    .AddApplicationServicesRegistration()
    .AddInfrastructureServicesRegistration(builder.Configuration);

#endregion

var app = builder.Build();

#region Apply EF Core Migrations
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
}
#endregion

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


// ── Hangfire dashboard ────────────────────────────────────────────────────
// Secured behind Admin role — configured in the Web layer step.
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    // Authorization filter added in Web layer (requires Admin role)
    Authorization = []
});

// ── Register recurring jobs ────────────────────────────────────────────────
InfrastructureServicesRegistration.RegisterHangfireJobs();

// ── Remaining middleware added in Web layer step ───────────────────────────
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

app.Run();
