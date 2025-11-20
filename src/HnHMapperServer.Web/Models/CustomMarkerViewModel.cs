namespace HnHMapperServer.Web.Models;

/// <summary>
/// View model for custom markers in the UI
/// </summary>
public class CustomMarkerViewModel
{
    /// <summary>
    /// Marker ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Map ID
    /// </summary>
    public int MapId { get; set; }

    /// <summary>
    /// Grid coordinate X
    /// </summary>
    public int CoordX { get; set; }

    /// <summary>
    /// Grid coordinate Y
    /// </summary>
    public int CoordY { get; set; }

    /// <summary>
    /// Position X within grid (0-100)
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// Position Y within grid (0-100)
    /// </summary>
    public int Y { get; set; }

    /// <summary>
    /// Marker title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Optional description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Icon path
    /// </summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// Username of creator
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// UTC timestamp when placed (immutable)
    /// </summary>
    public DateTime PlacedAt { get; set; }

    /// <summary>
    /// UTC timestamp when last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Whether marker is hidden
    /// </summary>
    public bool Hidden { get; set; }

    /// <summary>
    /// Whether current user can edit this marker
    /// </summary>
    public bool CanEdit { get; set; }

    /// <summary>
    /// Timer countdown text for display on map (e.g., "2h 15m", "45m")
    /// </summary>
    public string? TimerText { get; set; }

    /// <summary>
    /// Get relative time string (e.g., "2 hours ago")
    /// </summary>
    public string RelativeTime => GetRelativeTime(PlacedAt);

    /// <summary>
    /// Calculate relative time from UTC timestamp
    /// </summary>
    private static string GetRelativeTime(DateTime dt)
    {
        if (dt <= DateTime.MinValue.AddSeconds(1))
        {
            return "–";
        }

        var utc = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
        var span = DateTime.UtcNow - utc;
        
        if (span.TotalMinutes < 1) return "just now";
        if (span.TotalMinutes < 2) return "1 minute ago";
        if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} minutes ago";
        if (span.TotalHours < 2) return "1 hour ago";
        if (span.TotalHours < 24) return $"{(int)span.TotalHours} hours ago";
        if (span.TotalDays < 2) return "yesterday";
        if (span.TotalDays < 7) return $"{(int)span.TotalDays} days ago";
        if (span.TotalDays < 14) return "last week";
        if (span.TotalDays < 30) return $"{(int)(span.TotalDays / 7)} weeks ago";
        
        return utc.ToLocalTime().ToString("MMM d, yyyy");
    }
}

/// <summary>
/// Event model for SSE custom marker events
/// </summary>
public class CustomMarkerEventModel
{
    public int Id { get; set; }
    public int MapId { get; set; }
    public string GridId { get; set; } = string.Empty;
    public int CoordX { get; set; }
    public int CoordY { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime PlacedAt { get; set; }
    public bool Hidden { get; set; }
}



