
using GymManagement.Domain.Results;

namespace GymManagement.Domain.Interfaces;

public interface IIdentityService
{
    Task<Result<string>> CreateUserAsync(string email, string password, string firstName, string lastName);
    Task<Result> AddToRoleAsync(string userId, string role);
    Task<bool> UserExistsAsync(string email);
}
