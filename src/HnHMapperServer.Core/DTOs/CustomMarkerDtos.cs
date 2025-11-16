namespace HnHMapperServer.Core.DTOs;

/// <summary>
/// DTO for creating a new custom marker
/// </summary>
public class CreateCustomMarkerDto
{
    /// <summary>
    /// Map ID where the marker is placed
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
    /// Position X within the grid (0-100)
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// Position Y within the grid (0-100)
    /// </summary>
    public int Y { get; set; }

    /// <summary>
    /// Marker title (max 80 characters, required)
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Optional description (max 1000 characters)
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Icon name/path (must be from allowed icon list)
    /// </summary>
    public string Icon { get; set; } = string.Empty;
}

/// <summary>
/// DTO for updating an existing custom marker
/// </summary>
public class UpdateCustomMarkerDto
{
    /// <summary>
    /// Marker title (max 80 characters, required)
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Optional description (max 1000 characters)
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Icon name/path (must be from allowed icon list)
    /// </summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// Whether the marker is hidden
    /// </summary>
    public bool Hidden { get; set; }
}

/// <summary>
/// DTO for viewing/listing custom markers
/// </summary>
public class CustomMarkerViewDto
{
    /// <summary>
    /// Marker ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Map ID where the marker is placed
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
    /// Position X within the grid (0-100)
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// Position Y within the grid (0-100)
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
    /// Icon name/path
    /// </summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// Username of the creator
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// UTC timestamp when the marker was placed
    /// </summary>
    public DateTime PlacedAt { get; set; }

    /// <summary>
    /// UTC timestamp when the marker was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Whether the marker is hidden
    /// </summary>
    public bool Hidden { get; set; }

    /// <summary>
    /// Whether the current user can edit this marker
    /// </summary>
    public bool CanEdit { get; set; }
}

/// <summary>
/// DTO for custom marker SSE events (lighter payload for real-time updates)
/// </summary>
public class CustomMarkerEventDto
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
    /// Grid ID
    /// </summary>
    public string GridId { get; set; } = string.Empty;

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
    /// Icon name/path
    /// </summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// Username of creator
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// UTC timestamp when placed
    /// </summary>
    public DateTime PlacedAt { get; set; }

    /// <summary>
    /// Whether marker is hidden
    /// </summary>
    public bool Hidden { get; set; }

    /// <summary>
    /// Tenant ID for multi-tenancy isolation
    /// </summary>
    public string TenantId { get; set; } = string.Empty;
}

/// <summary>
/// DTO for custom marker delete events (SSE)
/// </summary>
public class CustomMarkerDeleteEventDto
{
    /// <summary>
    /// Marker ID that was deleted
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Tenant ID for multi-tenancy isolation
    /// </summary>
    public string TenantId { get; set; } = string.Empty;
}

