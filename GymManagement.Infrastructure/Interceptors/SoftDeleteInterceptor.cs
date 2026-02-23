using GymManagement.Domain.Bases;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace GymManagement.Infrastructure.Interceptors
{
    public class SoftDeleteInterceptor : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken ct = default)
        {
            if (eventData.Context is null) return base.SavingChangesAsync(eventData, result, ct);

            var entries = eventData.Context.ChangeTracker
                .Entries<SoftDeletableEntity>()
                .Where(e => e.State == EntityState.Deleted);

            foreach (var entry in entries)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedAt = DateTime.UtcNow;
                
                // Note: If you have an IHttpContextAccessor here, 
                // you could also set entry.Entity.DeletedById automatically.
            }

            return base.SavingChangesAsync(eventData, result, ct);
        }
    }
}
