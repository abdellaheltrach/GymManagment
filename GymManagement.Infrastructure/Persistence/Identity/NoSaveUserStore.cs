using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using GymManagement.Domain.Entities.Identity;
using GymManagement.Infrastructure.Context;

namespace GymManagement.Infrastructure.Persistence.Identity;

/// <summary>
/// Custom UserStore with AutoSaveChanges = false.
/// Prevents Identity from calling SaveChangesAsync automatically after every
/// write (CreateAsync, UpdateAsync, DeleteAsync, AddToRoleAsync, etc.).
/// TransactionBehaviour calls SaveChangesAsync once after the handler completes,
/// flushing Identity rows and domain entities atomically in one transaction.
/// </summary>
public class NoSaveUserStore : UserStore<ApplicationUser>
{
    public NoSaveUserStore(AppDbContext context, IdentityErrorDescriber? describer = null)
        : base(context, describer)
    {
        AutoSaveChanges = false;
    }
}
