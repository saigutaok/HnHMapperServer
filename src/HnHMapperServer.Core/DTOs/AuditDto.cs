namespace HnHMapperServer.Core.DTOs;

/// <summary>
/// DTO for creating audit log entries.
/// Used by IAuditService.LogAsync()
/// </summary>
public class AuditEntry
{
    /// <summary>
    /// Timestamp when the action occurred (defaults to UTC now)
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User ID who performed the action (NULL for system actions)
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Tenant ID where the action occurred (NULL for global actions)
    /// </summary>
    public string? TenantId { get; set; }

    /// <summary>
    /// Action performed (e.g., "UpdateUserPermissions", "CreateTenant")
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Type of entity affected (e.g., "TenantUser", "Tenant", "Token")
    /// </summary>
    public string? EntityType { get; set; }

    /// <summary>
    /// ID of the affected entity
    /// </summary>
    public string? EntityId { get; set; }

    /// <summary>
    /// JSON snapshot before the change (NULL for create operations)
    /// </summary>
    public string? OldValue { get; set; }

    /// <summary>
    /// JSON snapshot after the change (NULL for delete operations)
    /// </summary>
    public string? NewValue { get; set; }

    /// <summary>
    /// IP address (auto-filled by AuditService if not provided)
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User-Agent (auto-filled by AuditService if not provided)
    /// </summary>
    public string? UserAgent { get; set; }
}

/// <summary>
/// Query parameters for filtering audit logs.
/// Used by IAuditService.GetLogsAsync()
/// </summary>
public class AuditQuery
{
    /// <summary>
    /// Filter by tenant ID (NULL = no filter)
    /// </summary>
    public string? TenantId { get; set; }

    /// <summary>
    /// Filter by user ID (NULL = no filter)
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Filter by action type (NULL = no filter)
    /// </summary>
    public string? Action { get; set; }

    /// <summary>
    /// Filter by entity type (NULL = no filter)
    /// </summary>
    public string? EntityType { get; set; }

    /// <summary>
    /// Filter by minimum timestamp (NULL = no filter)
    /// </summary>
    public DateTime? Since { get; set; }

    /// <summary>
    /// Filter by maximum timestamp (NULL = no filter)
    /// </summary>
    public DateTime? Until { get; set; }

    /// <summary>
    /// Maximum number of results to return (default: 100)
    /// </summary>
    public int Limit { get; set; } = 100;
}

/// <summary>
/// DTO for returning audit logs to API clients.
/// Enriched with resolved user/tenant names.
/// </summary>
public class AuditLogDto
{
    /// <summary>
    /// Audit log ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// When the action occurred (UTC)
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// User ID who performed the action
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Username (resolved from UserId, may be null if user deleted)
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Tenant ID where action occurred
    /// </summary>
    public string? TenantId { get; set; }

    /// <summary>
    /// Tenant name (resolved from TenantId, may be null if tenant deleted)
    /// </summary>
    public string? TenantName { get; set; }

    /// <summary>
    /// Action performed
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Type of entity affected
    /// </summary>
    public string? EntityType { get; set; }

    /// <summary>
    /// ID of the affected entity
    /// </summary>
    public string? EntityId { get; set; }

    /// <summary>
    /// JSON snapshot before change
    /// </summary>
    public string? OldValue { get; set; }

    /// <summary>
    /// JSON snapshot after change
    /// </summary>
    public string? NewValue { get; set; }

    /// <summary>
    /// IP address of the client
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User-Agent of the client
    /// </summary>
    public string? UserAgent { get; set; }
}
