using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using GymManagement.Infrastructure.Context;

namespace GymManagement.Infrastructure.Persistence.Identity;

/// <summary>
/// Custom RoleStore with AutoSaveChanges = false.
/// Same reason as NoSaveUserStore — role write operations must not
/// auto-commit so they participate in TransactionBehaviour's transaction.
/// </summary>
public class NoSaveRoleStore : RoleStore<IdentityRole>
{
    public NoSaveRoleStore(AppDbContext context, IdentityErrorDescriber? describer = null)
        : base(context, describer)
    {
        AutoSaveChanges = false;
    }
}
