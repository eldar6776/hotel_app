using Microsoft.AspNetCore.Http;
using HotelPro.Core.Attributes;
using HotelPro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Text.Json;

namespace HotelPro.Infrastructure.Data.Interceptors;

public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public AuditInterceptor(IHttpContextAccessor? httpContextAccessor = null)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context == null) return await base.SavingChangesAsync(eventData, result, cancellationToken);

        var auditLogs = new List<AuditLog>();

        foreach (var entry in context.ChangeTracker.Entries())
        {
            var entityType = entry.Metadata.ClrType;
            var auditableAttr = entityType.GetCustomAttributes(typeof(AuditableAttribute), true).FirstOrDefault() as AuditableAttribute;
            if (auditableAttr == null || !auditableAttr.TrackChanges) continue;
            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted)) continue;

            var excludeProps = auditableAttr.ExcludeProperties ?? Array.Empty<string>();

            var oldValues = entry.State == EntityState.Added ? null :
                entry.Properties.Where(p => !excludeProps.Contains(p.Metadata.Name))
                    .ToDictionary(p => p.Metadata.Name, p => p.OriginalValue);

            var newValues = entry.State == EntityState.Deleted ? null :
                entry.Properties.Where(p => !excludeProps.Contains(p.Metadata.Name))
                    .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);

            var changedProps = entry.State == EntityState.Modified
                ? entry.Properties.Where(p => !Equals(p.OriginalValue, p.CurrentValue) && !excludeProps.Contains(p.Metadata.Name))
                    .Select(p => p.Metadata.Name)
                : null;

            var entityId = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "Id")?.CurrentValue?.ToString() ?? string.Empty;

            var userIdClaim = _httpContextAccessor?.HttpContext?.User?.FindFirst("sub")?.Value;
            var emailClaim = _httpContextAccessor?.HttpContext?.User?.FindFirst("email")?.Value;

            auditLogs.Add(new AuditLog
            {
                Id = Guid.NewGuid(),
                EntityName = entityType.Name,
                EntityId = entityId,
                Action = entry.State.ToString(),
                OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
                NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
                ChangedProperties = changedProps != null ? JsonSerializer.Serialize(changedProps) : null,
                ChangedById = Guid.TryParse(userIdClaim, out var userId) ? userId : null,
                ChangedByEmail = emailClaim,
                ChangedAt = DateTime.UtcNow,
                IpAddress = _httpContextAccessor?.HttpContext?.Connection.RemoteIpAddress?.ToString()
            });
        }

        foreach (var log in auditLogs)
        {
            context.Set<AuditLog>().Add(log);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
