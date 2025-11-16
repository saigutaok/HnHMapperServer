namespace HnHMapperServer.Core.Models;

/// <summary>
/// Represents tile data including zoom levels and file paths
/// </summary>
public class TileData
{
    public int MapId { get; set; }
    public Coord Coord { get; set; } = new(0, 0);
    public int Zoom { get; set; }
    public string File { get; set; } = string.Empty;
    public long Cache { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public int FileSizeBytes { get; set; }
}
