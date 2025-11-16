using HnHMapperServer.Core.Models;

namespace HnHMapperServer.Core.Interfaces;

/// <summary>
/// Repository interface for custom marker operations
/// </summary>
public interface ICustomMarkerRepository
{
    /// <summary>
    /// Get a custom marker by ID
    /// </summary>
    Task<CustomMarker?> GetByIdAsync(int id);

    /// <summary>
    /// Get all custom markers for a specific map
    /// </summary>
    Task<List<CustomMarker>> GetByMapIdAsync(int mapId);

    /// <summary>
    /// Get all custom markers created by a specific user
    /// </summary>
    Task<List<CustomMarker>> GetByCreatorAsync(string username);

    /// <summary>
    /// Create a new custom marker
    /// </summary>
    Task<CustomMarker> CreateAsync(CustomMarker marker);

    /// <summary>
    /// Update an existing custom marker
    /// </summary>
    Task<CustomMarker> UpdateAsync(CustomMarker marker);

    /// <summary>
    /// Delete a custom marker
    /// </summary>
    Task DeleteAsync(int id);

    /// <summary>
    /// Check if a custom marker exists
    /// </summary>
    Task<bool> ExistsAsync(int id);
}

