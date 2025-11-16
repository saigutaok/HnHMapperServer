namespace HnHMapperServer.Core.DTOs;

/// <summary>
/// DTO for map revision updates sent via SSE.
/// Clients use this to know when to invalidate cached tiles.
/// </summary>
public class MapRevisionDto
{
    /// <summary>
    /// Map ID that was updated
    /// </summary>
    public int MapId { get; set; }

    /// <summary>
    /// New revision number (incremented on any tile-affecting operation)
    /// </summary>
    public int Revision { get; set; }
}

