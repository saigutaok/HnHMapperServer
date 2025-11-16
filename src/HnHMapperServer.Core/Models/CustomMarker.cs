namespace HnHMapperServer.Core.Models;

/// <summary>
/// Domain model for a custom marker created by users on the map
/// </summary>
public class CustomMarker
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
    /// Grid ID (string identifier)
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
    /// Optional description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Icon name/path
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
}



