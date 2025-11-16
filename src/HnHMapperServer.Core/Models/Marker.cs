namespace HnHMapperServer.Core.Models;

/// <summary>
/// Represents a marker placed on the map (e.g., resources, points of interest)
/// </summary>
public class Marker
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string GridId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public Position Position { get; set; } = new(0, 0);
    public string Image { get; set; } = string.Empty;
    public bool Hidden { get; set; }
    public long MaxReady { get; set; } = -1;
    public long MinReady { get; set; } = -1;
    public bool Ready { get; set; }
}

/// <summary>
/// Frontend representation of a marker with map information
/// </summary>
public class FrontendMarker
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Map { get; set; }
    public Position Position { get; set; } = new(0, 0);
    public string Image { get; set; } = string.Empty;
    public bool Hidden { get; set; }
    public long MaxReady { get; set; } = -1;
    public long MinReady { get; set; } = -1;
    public bool Ready { get; set; }
}
