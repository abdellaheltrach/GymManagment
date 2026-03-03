using GymManagement.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Infrastructure.Persistence.Seeders
{
    public static class RoleSeeder
    {
        public static async Task SeedAsync(RoleManager<IdentityRole> roleManager)
        {
            if (await roleManager.Roles.CountAsync() <= 0)
            {
                await roleManager.CreateAsync(new IdentityRole { Name = "Admin" });
                await roleManager.CreateAsync(new IdentityRole { Name = "Trainer" });
                await roleManager.CreateAsync(new IdentityRole { Name = "Receptionist" });
                await roleManager.CreateAsync(new IdentityRole { Name = "Trainee" });
            }
        }
    }
}
