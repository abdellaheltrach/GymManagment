using GymManagement.Domain.Entities.Identity;
using GymManagement.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Infrastructure.Persistence.Seeders
{
    public static class UserSeeder
    {
        public static async Task SeedAsync(UserManager<ApplicationUser> userManager, IUnitOfWork uow)
        {
            if (await userManager.Users.CountAsync() <= 0)
            {
                var defaultUser = new ApplicationUser
                {
                    UserName = "admin",
                    Email = "admin@project.com",
                    FirstName = "System",
                    LastName = "Admin",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    IsActive = true
                };

                // CRITICAL: Password must be at least 12 characters to pass the configured policy
                var result = await userManager.CreateAsync(defaultUser, "M12345789_m!");
                
                if (result.Succeeded)
                {
                    // CRITICAL: We must save changes here because AutoSaveChanges is disabled in this project.
                    // Otherwise AddToRoleAsync will fail with a Foreign Key constraint error.
                    await uow.SaveChangesAsync(); 
                    
                    await userManager.AddToRoleAsync(defaultUser, "Admin");
                    await uow.SaveChangesAsync();
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new Exception($"Failed to seed admin user: {errors}");
                }
            }
        }
    }
}
