using GymManagement.Domain.Entities.Identity;
using GymManagement.Domain.Interfaces;
using GymManagement.Domain.Results;
using Microsoft.AspNetCore.Identity;

namespace GymManagement.Infrastructure.Identity
{
    public class IdentityService(
        UserManager<ApplicationUser> userManager) : IIdentityService
    {
        public async Task<Result<string>> CreateUserAsync(string email, string password, string firstName, string lastName)
        {
            var user = new ApplicationUser
            {
                UserName = email, // Using email as username for consistency
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                IsActive = true
            };

            var result = await userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                return Result<string>.Success(user.Id);
            }

            return Result<string>.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        public async Task<Result> AddToRoleAsync(string userId, string role)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return Result.NotFound("User not found.");

            var result = await userManager.AddToRoleAsync(user, role);

            return result.Succeeded
                ? Result.Success()
                : Result.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            return await userManager.FindByEmailAsync(email) != null;
        }

        public async Task<Result> DeleteUserAsync(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user is null) return Result.Success(); // already gone, nothing to do

            var result = await userManager.DeleteAsync(user);
            return result.Succeeded
                ? Result.Success()
                : Result.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
}
