using GymManagement.Domain.Entities.Identity;
using GymManagement.Infrastructure.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GymManagement.Infrastructure.Middleware
{
    /// <summary>
    /// Per-request middleware for authenticated Receptionists.
    /// Compares DB permission bitmask vs cookie claim.
    /// If changed: refreshes cookie. If deactivated: forces sign-out.
    /// </summary>
    public class ReceptionistPermissionRefreshMiddleware
    {
        private readonly RequestDelegate _next;

        public ReceptionistPermissionRefreshMiddleware(RequestDelegate next)
            => _next = next;

        public async Task InvokeAsync(
            HttpContext context,
            AppDbContext db,
            SignInManager<ApplicationUser> signInManager)
        {
            var user = context.User;

            // Skip: unauthenticated, admins, trainers
            if (user.Identity?.IsAuthenticated != true
                || !user.IsInRole("Receptionist"))
            {
                await _next(context);
                return;
            }

            var claim = user.FindFirst("receptionist_permissions");
            if (claim is null || !int.TryParse(claim.Value, out var cookieBitmask))
            {
                await _next(context);
                return;
            }

            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId is null)
            {
                await _next(context);
                return;
            }

            var dbBitmask = await db.Receptionists
                .AsNoTracking()
                .Where(r => r.ApplicationUserId == userId && r.IsActive)
                .Select(r => (int?)r.Permissions)
                .FirstOrDefaultAsync();

            // Receptionist deactivated or not found → sign out
            if (dbBitmask is null)
            {
                await signInManager.SignOutAsync();
                context.Response.Redirect("/Account/Login");
                return;
            }

            // Permissions changed → refresh cookie
            if (dbBitmask.Value != cookieBitmask)
            {
                var appUser = await signInManager.UserManager.GetUserAsync(user);
                if (appUser is not null)
                    await signInManager.RefreshSignInAsync(appUser);
            }

            await _next(context);
        }
    }
}
