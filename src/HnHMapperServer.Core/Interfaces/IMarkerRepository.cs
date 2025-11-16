using HnHMapperServer.Core.Models;

namespace HnHMapperServer.Core.Interfaces;

public interface IMarkerRepository
{
    Task<Marker?> GetMarkerAsync(int markerId);
    Task<Marker?> GetMarkerByKeyAsync(string key);
    Task<List<Marker>> GetAllMarkersAsync();
    Task SaveMarkerAsync(Marker marker, string key);
    Task DeleteMarkerAsync(string key);
    Task<int> GetNextMarkerIdAsync();
}
