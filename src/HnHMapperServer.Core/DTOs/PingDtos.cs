namespace HnHMapperServer.Core.DTOs;

/// <summary>
/// DTO for creating a new ping
/// </summary>
public class CreatePingDto
{
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
    /// Position X within the grid (0-100)
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// Position Y within the grid (0-100)
    /// </summary>
    public int Y { get; set; }
}

/// <summary>
/// DTO for ping SSE events (lightweight payload for real-time updates)
/// </summary>
public class PingEventDto
{
    /// <summary>
    /// Ping ID
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
    /// Username of creator
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// UTC timestamp when ping was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// UTC timestamp when ping expires
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Tenant ID for multi-tenancy isolation
    /// </summary>
    public string TenantId { get; set; } = string.Empty;
}

/// <summary>
/// DTO for ping delete events (SSE)
/// </summary>
public class PingDeleteEventDto
{
    /// <summary>
    /// Ping ID that was deleted
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Tenant ID for multi-tenancy isolation
    /// </summary>
    public string TenantId { get; set; } = string.Empty;
}
