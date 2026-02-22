using GymManagement.Domain.Bases;
using GymManagement.Domain.Entities;
using GymManagement.Domain.Enums;
using GymManagement.Infrastructure.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Text.Json;

namespace GymManagement.Infrastructure.Interceptors
{
    public class AuditInterceptor : SaveChangesInterceptor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Properties that must never be serialised into audit snapshots
        private static readonly HashSet<string> ExcludedProperties =
        [
            "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
        "RowVersion", "NormalizedEmail", "NormalizedUserName"
        ];

        public AuditInterceptor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken ct = default)
        {
            if (eventData.Context is AppDbContext context)
                await WriteAuditLogsAsync(context, ct);

            return await base.SavingChangesAsync(eventData, result, ct);
        }

        private async Task WriteAuditLogsAsync(AppDbContext context, CancellationToken ct)
        {
            var userId = _httpContextAccessor.HttpContext?.User?
                .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var auditEntries = context.ChangeTracker
                .Entries<BaseEntity>()
                .Where(e => e.State is EntityState.Added
                                  or EntityState.Modified
                                  or EntityState.Deleted)
                .Select(entry => BuildAuditLog(entry, userId))
                .Where(a => a is not null)
                .Cast<AuditLog>()
                .ToList();

            if (auditEntries.Count > 0)
                await context.AuditLogs.AddRangeAsync(auditEntries, ct);
        }

        private static AuditLog? BuildAuditLog(
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<BaseEntity> entry,
            string? userId)
        {
            var entityName = entry.Entity.GetType().Name;
            var entityId = entry.Entity.Id.ToString();

            var action = entry.State switch
            {
                EntityState.Added => AuditAction.Created,
                EntityState.Modified => AuditAction.Updated,
                EntityState.Deleted => AuditAction.Deleted,
                _ => (AuditAction?)null
            };

            if (action is null) return null;

            string? oldValues = null;
            string? newValues = null;

            if (action == AuditAction.Updated)
            {
                var changed = entry.Properties
                    .Where(p => p.IsModified && !ExcludedProperties.Contains(p.Metadata.Name))
                    .ToList();

                if (changed.Count == 0) return null; // nothing meaningful changed

                oldValues = JsonSerializer.Serialize(
                    changed.ToDictionary(p => p.Metadata.Name, p => p.OriginalValue));

                newValues = JsonSerializer.Serialize(
                    changed.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue));
            }
            else if (action == AuditAction.Created)
            {
                newValues = JsonSerializer.Serialize(
                    entry.Properties
                         .Where(p => !ExcludedProperties.Contains(p.Metadata.Name))
                         .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue));
            }
            else // Deleted
            {
                oldValues = JsonSerializer.Serialize(
                    entry.Properties
                         .Where(p => !ExcludedProperties.Contains(p.Metadata.Name))
                         .ToDictionary(p => p.Metadata.Name, p => p.OriginalValue));
            }

            return new AuditLog
            {
                EntityName = entityName,
                EntityId = entityId,
                Action = action.Value,
                OldValues = oldValues,
                NewValues = newValues,
                ChangedById = userId,
                ChangedAt = DateTime.UtcNow
            };
        }
    }
}