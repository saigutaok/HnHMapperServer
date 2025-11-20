namespace HnHMapperServer.Core.DTOs;

/// <summary>
/// DTO for creating a new notification
/// </summary>
public class CreateNotificationDto
{
    /// <summary>
    /// Tenant ID for multi-tenancy isolation
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// User ID who should receive this notification (NULL = all users in tenant)
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Type of notification (MarkerTimerExpired, StandaloneTimerExpired, etc.)
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Notification title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Notification message/body
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Action type when notification is clicked
    /// </summary>
    public string? ActionType { get; set; }

    /// <summary>
    /// JSON string with action parameters
    /// </summary>
    public string? ActionData { get; set; }

    /// <summary>
    /// Priority level (Low, Normal, High, Urgent)
    /// </summary>
    public string Priority { get; set; } = "Normal";

    /// <summary>
    /// When the notification expires (NULL = never expires)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
}

/// <summary>
/// DTO for notification data returned to clients
/// </summary>
public class NotificationDto
{
    /// <summary>
    /// Notification ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Tenant ID
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// User ID (NULL = all users)
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Type of notification
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Notification title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Notification message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Action type
    /// </summary>
    public string? ActionType { get; set; }

    /// <summary>
    /// Action data (JSON)
    /// </summary>
    public string? ActionData { get; set; }

    /// <summary>
    /// Whether the notification has been read
    /// </summary>
    public bool IsRead { get; set; }

    /// <summary>
    /// When the notification was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the notification expires
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Priority level
    /// </summary>
    public string Priority { get; set; } = "Normal";
}

/// <summary>
/// DTO for notification SSE events (lightweight payload for real-time updates)
/// </summary>
public class NotificationEventDto
{
    /// <summary>
    /// Notification ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Tenant ID
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// User ID (NULL = all users)
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Type of notification
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Notification title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Notification message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Action type
    /// </summary>
    public string? ActionType { get; set; }

    /// <summary>
    /// Action data (JSON)
    /// </summary>
    public string? ActionData { get; set; }

    /// <summary>
    /// Priority level
    /// </summary>
    public string Priority { get; set; } = "Normal";

    /// <summary>
    /// When the notification was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for querying notifications
/// </summary>
public class NotificationQuery
{
    /// <summary>
    /// Filter by tenant ID
    /// </summary>
    public string? TenantId { get; set; }

    /// <summary>
    /// Filter by user ID
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Filter by notification type
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Filter by read status (NULL = all, true = read only, false = unread only)
    /// </summary>
    public bool? IsRead { get; set; }

    /// <summary>
    /// Include expired notifications
    /// </summary>
    public bool IncludeExpired { get; set; } = false;

    /// <summary>
    /// Maximum number of results
    /// </summary>
    public int Limit { get; set; } = 50;

    /// <summary>
    /// Offset for pagination
    /// </summary>
    public int Offset { get; set; } = 0;
}
