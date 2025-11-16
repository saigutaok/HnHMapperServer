using HnHMapperServer.Core.DTOs;
using HnHMapperServer.Core.Models;
using HnHMapperServer.Infrastructure.Data;
using HnHMapperServer.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HnHMapperServer.Services.Services;

/// <summary>
/// Implementation of audit logging service.
/// Automatically captures IP address and User-Agent from HttpContext.
/// </summary>
public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(
        ApplicationDbContext db,
        IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Log an audit entry.
    /// Auto-fills IP address and User-Agent from current HTTP request if available.
    /// </summary>
    public async Task LogAsync(AuditEntry entry)
    {
        // Auto-fill IP and User-Agent from current request if available and not already set
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            entry.IpAddress ??= httpContext.Connection.RemoteIpAddress?.ToString();
            entry.UserAgent ??= httpContext.Request.Headers["User-Agent"].ToString();
        }

        // Convert to entity
        var auditLog = new AuditLogEntity
        {
            Timestamp = entry.Timestamp.ToString("O"),  // ISO 8601 format
            UserId = entry.UserId,
            TenantId = entry.TenantId,
            Action = entry.Action,
            EntityType = entry.EntityType,
            EntityId = entry.EntityId,
            OldValue = entry.OldValue,
            NewValue = entry.NewValue,
            IpAddress = entry.IpAddress,
            UserAgent = entry.UserAgent
        };

        _db.AuditLogs.Add(auditLog);
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Query audit logs with filtering.
    /// </summary>
    public async Task<List<AuditEntry>> GetLogsAsync(AuditQuery query)
    {
        var logsQuery = _db.AuditLogs.AsQueryable();

        // Apply filters
        if (query.TenantId != null)
            logsQuery = logsQuery.Where(l => l.TenantId == query.TenantId);

        if (query.UserId != null)
            logsQuery = logsQuery.Where(l => l.UserId == query.UserId);

        if (query.Action != null)
            logsQuery = logsQuery.Where(l => l.Action == query.Action);

        if (query.EntityType != null)
            logsQuery = logsQuery.Where(l => l.EntityType == query.EntityType);

        if (query.Since != null)
            logsQuery = logsQuery.Where(l => DateTime.Parse(l.Timestamp) >= query.Since.Value);

        if (query.Until != null)
            logsQuery = logsQuery.Where(l => DateTime.Parse(l.Timestamp) <= query.Until.Value);

        // Execute query with ordering and limit
        var logs = await logsQuery
            .OrderByDescending(l => l.Timestamp)
            .Take(query.Limit)
            .ToListAsync();

        // Map to DTOs
        return logs.Select(MapToAuditEntry).ToList();
    }

    /// <summary>
    /// Get audit logs for a specific tenant.
    /// </summary>
    public async Task<List<AuditEntry>> GetTenantLogsAsync(
        string tenantId,
        DateTime? since = null,
        int limit = 100)
    {
        return await GetLogsAsync(new AuditQuery
        {
            TenantId = tenantId,
            Since = since,
            Limit = limit
        });
    }

    /// <summary>
    /// Get complete history of an entity.
    /// </summary>
    public async Task<List<AuditEntry>> GetEntityHistoryAsync(
        string entityType,
        string entityId)
    {
        var logs = await _db.AuditLogs
            .Where(l => l.EntityType == entityType && l.EntityId == entityId)
            .OrderBy(l => l.Timestamp)  // Chronological order for history
            .ToListAsync();

        return logs.Select(MapToAuditEntry).ToList();
    }

    /// <summary>
    /// Map AuditLogEntity to AuditEntry DTO.
    /// </summary>
    private static AuditEntry MapToAuditEntry(AuditLogEntity entity)
    {
        return new AuditEntry
        {
            Timestamp = DateTime.Parse(entity.Timestamp),
            UserId = entity.UserId,
            TenantId = entity.TenantId,
            Action = entity.Action,
            EntityType = entity.EntityType,
            EntityId = entity.EntityId,
            OldValue = entity.OldValue,
            NewValue = entity.NewValue,
            IpAddress = entity.IpAddress,
            UserAgent = entity.UserAgent
        };
    }
}
