namespace HnHMapperServer.Core.Models;

/// <summary>
/// Represents map metadata and configuration
/// </summary>
public class MapInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool Hidden { get; set; }
    public int Priority { get; set; }

    /// <summary>
    /// UTC timestamp when the map was created
    /// Used for auto-cleanup of empty maps
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Default starting X coordinate when opening this map without URL parameters
    /// </summary>
    public int? DefaultStartX { get; set; }

    /// <summary>
    /// Default starting Y coordinate when opening this map without URL parameters
    /// </summary>
    public int? DefaultStartY { get; set; }

    public string TenantId { get; set; } = string.Empty;
}
