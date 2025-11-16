namespace HnHMapperServer.Web.Models;

/// <summary>
/// Event model for SSE ping events
/// </summary>
public class PingEventModel
{
    public int Id { get; set; }
    public int MapId { get; set; }
    public int CoordX { get; set; }
    public int CoordY { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string TenantId { get; set; } = string.Empty;
}

/// <summary>
/// Delete event model for ping deletion SSE events
/// </summary>
public class PingDeleteEventModel
{
    public int Id { get; set; }
    public string TenantId { get; set; } = string.Empty;
}
