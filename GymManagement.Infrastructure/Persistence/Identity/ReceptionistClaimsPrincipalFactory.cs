using GymManagement.Domain.Entities.Identity;
using GymManagement.Infrastructure.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace GymManagement.Infrastructure.Persistence.Identity
{
    /// <summary>
    /// Adds the "receptionist_permissions" claim to the cookie when a
    /// Receptionist logs in. Called automatically by SignInManager.
    /// Admins and Trainers receive no extra claims — this is a no-op for them.
    /// </summary>
    public class ReceptionistClaimsPrincipalFactory
        : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
    {
        private readonly AppDbContext _db;

        public ReceptionistClaimsPrincipalFactory(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<IdentityOptions> options,
            AppDbContext db)
            : base(userManager, roleManager, options)
        {
            _db = db;
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(
            ApplicationUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);

            var isReceptionist = await UserManager.IsInRoleAsync(user, "Receptionist");
            if (!isReceptionist)
                return identity;

            var receptionist = await _db.Receptionists
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.ApplicationUserId == user.Id);

            if (receptionist is null)
                return identity;

            identity.AddClaim(new Claim(
                "receptionist_permissions",
                receptionist.Permissions.ToString()));

            return identity;
        }
    }
}
