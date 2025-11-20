namespace HnHMapperServer.Core.DTOs;

/// <summary>
/// DTO for creating a new timer
/// </summary>
public class CreateTimerDto
{
    /// <summary>
    /// Type of timer (Marker, Standalone)
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Marker ID (for marker timers, NULL for standalone)
    /// </summary>
    public int? MarkerId { get; set; }

    /// <summary>
    /// Custom marker ID (for custom marker timers, NULL otherwise)
    /// </summary>
    public int? CustomMarkerId { get; set; }

    /// <summary>
    /// Timer title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Optional description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// When the timer expires (UTC timestamp)
    /// </summary>
    public DateTime ReadyAt { get; set; }
}

/// <summary>
/// DTO for updating an existing timer
/// </summary>
public class UpdateTimerDto
{
    /// <summary>
    /// New timer title
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// New description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// New ready-at time
    /// </summary>
    public DateTime? ReadyAt { get; set; }
}

/// <summary>
/// DTO for timer data returned to clients
/// </summary>
public class TimerDto
{
    /// <summary>
    /// Timer ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Tenant ID
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// User ID who created the timer
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Type of timer (Marker, Standalone)
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Marker ID (NULL for standalone timers)
    /// </summary>
    public int? MarkerId { get; set; }

    /// <summary>
    /// Custom marker ID (NULL for resource markers or standalone)
    /// </summary>
    public int? CustomMarkerId { get; set; }

    /// <summary>
    /// Timer title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Optional description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// When the timer expires
    /// </summary>
    public DateTime ReadyAt { get; set; }

    /// <summary>
    /// When the timer was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Whether the timer has completed
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// When the timer was completed (if applicable)
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Whether notification has been sent
    /// </summary>
    public bool NotificationSent { get; set; }

    /// <summary>
    /// Time remaining in seconds (negative if expired)
    /// </summary>
    public long TimeRemainingSeconds { get; set; }

    /// <summary>
    /// Whether the timer is ready (expired)
    /// </summary>
    public bool IsReady { get; set; }
}

/// <summary>
/// DTO for timer SSE events (lightweight payload for real-time updates)
/// </summary>
public class TimerEventDto
{
    /// <summary>
    /// Timer ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Tenant ID
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// User ID
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Type of timer
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Marker ID
    /// </summary>
    public int? MarkerId { get; set; }

    /// <summary>
    /// Custom marker ID
    /// </summary>
    public int? CustomMarkerId { get; set; }

    /// <summary>
    /// Timer title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// When the timer expires
    /// </summary>
    public DateTime ReadyAt { get; set; }

    /// <summary>
    /// Whether the timer is completed
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// When the timer was completed
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// When the timer was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for querying timers
/// </summary>
public class TimerQuery
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
    /// Filter by timer type
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Filter by marker ID
    /// </summary>
    public int? MarkerId { get; set; }

    /// <summary>
    /// Filter by custom marker ID
    /// </summary>
    public int? CustomMarkerId { get; set; }

    /// <summary>
    /// Filter by completion status (NULL = all, true = completed only, false = active only)
    /// </summary>
    public bool? IsCompleted { get; set; }

    /// <summary>
    /// Filter by ready status (NULL = all, true = ready/expired, false = pending)
    /// </summary>
    public bool? IsReady { get; set; }

    /// <summary>
    /// Maximum number of results
    /// </summary>
    public int Limit { get; set; } = 100;

    /// <summary>
    /// Offset for pagination
    /// </summary>
    public int Offset { get; set; } = 0;
}

/// <summary>
/// DTO for timer history entry
/// </summary>
public class TimerHistoryDto
{
    /// <summary>
    /// History entry ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Original timer ID
    /// </summary>
    public int TimerId { get; set; }

    /// <summary>
    /// Tenant ID
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// When the timer completed
    /// </summary>
    public DateTime CompletedAt { get; set; }

    /// <summary>
    /// Actual duration in minutes
    /// </summary>
    public int? Duration { get; set; }

    /// <summary>
    /// Type of timer
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Marker ID
    /// </summary>
    public int? MarkerId { get; set; }

    /// <summary>
    /// Custom marker ID
    /// </summary>
    public int? CustomMarkerId { get; set; }

    /// <summary>
    /// Timer title
    /// </summary>
    public string Title { get; set; } = string.Empty;
}
