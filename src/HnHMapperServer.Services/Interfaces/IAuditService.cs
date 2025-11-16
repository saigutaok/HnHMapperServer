using HnHMapperServer.Core.DTOs;

namespace HnHMapperServer.Services.Interfaces;

/// <summary>
/// Service for audit logging of sensitive operations.
/// Tracks who did what, when, and what changed.
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Log an audit entry for a sensitive operation.
    /// Automatically captures IP address and User-Agent from current HttpContext if available.
    /// </summary>
    /// <param name="entry">Audit entry with action details</param>
    /// <returns>Task</returns>
    Task LogAsync(AuditEntry entry);

    /// <summary>
    /// Query audit logs with filtering.
    /// </summary>
    /// <param name="query">Query parameters (tenant, user, action, date range, limit)</param>
    /// <returns>List of audit logs matching the query</returns>
    Task<List<AuditEntry>> GetLogsAsync(AuditQuery query);

    /// <summary>
    /// Get audit logs for a specific tenant.
    /// Convenience method for tenant-scoped audit viewing.
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="since">Optional start date (NULL = no filter)</param>
    /// <param name="limit">Maximum number of results (default: 100)</param>
    /// <returns>List of audit logs for the tenant</returns>
    Task<List<AuditEntry>> GetTenantLogsAsync(string tenantId, DateTime? since = null, int limit = 100);

    /// <summary>
    /// Get complete history of changes for a specific entity.
    /// Useful for tracking the lifecycle of a user, tenant, etc.
    /// </summary>
    /// <param name="entityType">Entity type (e.g., "TenantUser", "Tenant")</param>
    /// <param name="entityId">Entity ID</param>
    /// <returns>List of audit logs for the entity, ordered chronologically</returns>
    Task<List<AuditEntry>> GetEntityHistoryAsync(string entityType, string entityId);
}
