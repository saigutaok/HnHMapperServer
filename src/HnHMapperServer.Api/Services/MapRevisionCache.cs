using System.Collections.Concurrent;

namespace HnHMapperServer.Api.Services;

/// <summary>
/// In-memory cache for per-map revision numbers.
/// Used to append ?v=revision to tile URLs for efficient browser caching.
/// Revision increments on any tile-affecting operation (upload, merge, admin wipe).
/// Resets on API restart (acceptable as clients will refetch on next change).
/// </summary>
public class MapRevisionCache
{
    /// <summary>
    /// Thread-safe map from mapId → revision number.
    /// Revision starts at 1 and increments on each change.
    /// </summary>
    private readonly ConcurrentDictionary<int, int> _mapIdToRevision = new();

    /// <summary>
    /// Gets the current revision for a map.
    /// Returns 1 if map has not been seen yet (initial revision).
    /// </summary>
    /// <param name="mapId">The map ID</param>
    /// <returns>Current revision number (≥1)</returns>
    public int Get(int mapId)
    {
        return _mapIdToRevision.GetOrAdd(mapId, _ => 1);
    }

    /// <summary>
    /// Increments the revision for a map and returns the new value.
    /// Called after any tile-affecting operation (upload, merge, admin wipe).
    /// </summary>
    /// <param name="mapId">The map ID</param>
    /// <returns>The new revision number after increment</returns>
    public int Increment(int mapId)
    {
        return _mapIdToRevision.AddOrUpdate(mapId, 
            addValue: 2, // If first time seen, start at 2 (previous implicit 1 → 2)
            updateValueFactory: (_, currentValue) => currentValue + 1);
    }

    /// <summary>
    /// Gets revisions for all known maps.
    /// Used to send initial revisions to new SSE clients.
    /// </summary>
    /// <returns>Dictionary of mapId → revision</returns>
    public Dictionary<int, int> GetAll()
    {
        return new Dictionary<int, int>(_mapIdToRevision);
    }
}

