namespace HnHMapperServer.Core.Models;

/// <summary>
/// Audit trail for all sensitive operations
/// </summary>
public sealed class AuditLogEntity
{
    /// <summary>
    /// Auto-increment primary key
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// ISO 8601 UTC timestamp when action occurred (stored as TEXT in SQLite)
    /// </summary>
    public string Timestamp { get; set; } = string.Empty;

    /// <summary>
    /// User who performed action (NULL for system actions)
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Affected tenant (NULL for global actions)
    /// </summary>
    public string? TenantId { get; set; }

    /// <summary>
    /// Action type (e.g., 'CreateTenant', 'UpdatePermissions')
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Type of affected entity (e.g., 'Tenant', 'User', 'Token')
    /// </summary>
    public string? EntityType { get; set; }

    /// <summary>
    /// ID of affected entity
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
    /// IP address of client
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent of client
    /// </summary>
    public string? UserAgent { get; set; }
}
