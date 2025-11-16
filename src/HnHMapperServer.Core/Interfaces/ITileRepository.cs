using HnHMapperServer.Core.Models;

namespace HnHMapperServer.Core.Interfaces;

public interface ITileRepository
{
    Task<TileData?> GetTileAsync(int mapId, Coord coord, int zoom);
    Task SaveTileAsync(TileData tileData);
    Task<List<TileData>> GetAllTilesAsync();
    Task DeleteTilesByMapAsync(int mapId);
}
