using HnHMapperServer.Core.Models;
using HnHMapperServer.Core.DTOs;

namespace HnHMapperServer.Services.Interfaces;

public interface IGridService
{
    /// <summary>
    /// Processes a grid update from a client
    /// </summary>
    Task<GridRequestDto> ProcessGridUpdateAsync(GridUpdateDto gridUpdate, string gridStorage);

    /// <summary>
    /// Locates a grid and returns its map and coordinates
    /// </summary>
    Task<(int mapId, Coord coord)?> LocateGridAsync(string gridId);

    /// <summary>
    /// Deletes a map tile and updates zoom levels
    /// </summary>
    Task DeleteMapTileAsync(int mapId, Coord coord, string gridStorage);

    /// <summary>
    /// Sets new coordinates for a map (shifts all tiles)
    /// </summary>
    Task SetCoordinatesAsync(int mapId, Coord fromCoord, Coord toCoord, string gridStorage);

    /// <summary>
    /// Cleans up grids with duplicate coordinates
    /// </summary>
    Task<int> CleanupMultiIdGridsAsync(string gridStorage);
}
