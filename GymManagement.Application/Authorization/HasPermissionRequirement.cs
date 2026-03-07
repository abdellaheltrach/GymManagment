using GymManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace GymManagement.Application.Authorization
{
    /// <summary>
    /// Claim name added by ReceptionistClaimsPrincipalFactory on login.
    /// </summary>
    public static class ReceptionistClaims
    {
        public const string Permissions = "receptionist_permissions";
    }

    /// <summary>
    /// Authorization requirement that checks a specific bit in the
    /// "receptionist_permissions" claim. Admins always pass.
    /// </summary>
    public class HasPermissionRequirement : IAuthorizationRequirement
    {
        public ReceptionistPermission Permission { get; }

        public HasPermissionRequirement(ReceptionistPermission permission)
            => Permission = permission;
    }

    public class HasPermissionHandler
        : AuthorizationHandler<HasPermissionRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            HasPermissionRequirement requirement)
        {
            // Admins always bypass
            if (context.User.IsInRole("Admin"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            var claim = context.User.FindFirst(ReceptionistClaims.Permissions);
            if (claim is null || !int.TryParse(claim.Value, out var bitmask))
                return Task.CompletedTask;

            if ((bitmask & (int)requirement.Permission) == (int)requirement.Permission)
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
