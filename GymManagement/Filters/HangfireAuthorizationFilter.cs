using Hangfire.Dashboard;

namespace GymManagement.Web.Filters
{
    /// <summary>
    /// Secures the Hangfire dashboard — only authenticated Admin users may access it.
    /// Registered in Program.cs via DashboardOptions.Authorization.
    /// </summary>
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            return httpContext.User.Identity?.IsAuthenticated == true
                && httpContext.User.IsInRole("Admin");
        }
    }
}
