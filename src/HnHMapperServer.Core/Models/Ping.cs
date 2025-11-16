namespace HnHMapperServer.Core.Models;

/// <summary>
/// Domain model for a temporary ping marker placed by users on the map
/// Pings automatically expire after 60 seconds
/// </summary>
public class Ping
{
    /// <summary>
    /// Ping ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Map ID where the ping is placed
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
    /// Username of creator
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// UTC timestamp when ping was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// UTC timestamp when ping expires (CreatedAt + 60 seconds)
    /// </summary>
    public DateTime ExpiresAt { get; set; }
}
