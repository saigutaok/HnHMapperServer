namespace HnHMapperServer.Core.Models;

/// <summary>
/// Represents a grid coordinate in the map system
/// </summary>
public record Coord(int X, int Y)
{
    /// <summary>
    /// Gets the coordinate name in format "X_Y"
    /// </summary>
    public string Name() => $"{X}_{Y}";

    /// <summary>
    /// Gets the parent coordinate for zoom level calculation
    /// Divides coordinates by 2, adjusting for negative values
    /// </summary>
    public Coord Parent()
    {
        var x = X;
        var y = Y;
        if (x < 0) x--;
        if (y < 0) y--;
        return new Coord(x / 2, y / 2);
    }
}
