using System.Security.Claims;

namespace GymManagement.Web.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Gets the current user's ID from claims.
        /// Throws if no ID claim present — this should never happen for authenticated users.
        /// </summary>
        public static string GetUserId(this ClaimsPrincipal user)
            => user.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? throw new InvalidOperationException("User ID claim not found.");

        public static string GetUserEmail(this ClaimsPrincipal user)
            => user.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

        public static string GetUserName(this ClaimsPrincipal user)
            => user.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
    }

}
