using HnHMapperServer.Core.DTOs;

namespace HnHMapperServer.Services.Interfaces;

/// <summary>
/// Service interface for custom marker operations with validation and authorization
/// </summary>
public interface ICustomMarkerService
{
    /// <summary>
    /// Get all custom markers for a specific map
    /// </summary>
    /// <param name="mapId">Map ID</param>
    /// <param name="currentUsername">Current user's username</param>
    /// <param name="isAdmin">Whether the current user is an admin</param>
    Task<List<CustomMarkerViewDto>> GetByMapIdAsync(int mapId, string currentUsername, bool isAdmin);

    /// <summary>
    /// Get a custom marker by ID
    /// </summary>
    /// <param name="id">Marker ID</param>
    /// <param name="currentUsername">Current user's username</param>
    /// <param name="isAdmin">Whether the current user is an admin</param>
    Task<CustomMarkerViewDto?> GetByIdAsync(int id, string currentUsername, bool isAdmin);

    /// <summary>
    /// Create a new custom marker
    /// </summary>
    /// <param name="dto">Create marker DTO</param>
    /// <param name="currentUsername">Current user's username (will be set as creator)</param>
    /// <returns>Created marker view DTO</returns>
    Task<CustomMarkerViewDto> CreateAsync(CreateCustomMarkerDto dto, string currentUsername);

    /// <summary>
    /// Update an existing custom marker
    /// </summary>
    /// <param name="id">Marker ID</param>
    /// <param name="dto">Update marker DTO</param>
    /// <param name="currentUsername">Current user's username</param>
    /// <param name="isAdmin">Whether the current user is an admin</param>
    /// <returns>Updated marker view DTO</returns>
    Task<CustomMarkerViewDto> UpdateAsync(int id, UpdateCustomMarkerDto dto, string currentUsername, bool isAdmin);

    /// <summary>
    /// Delete a custom marker
    /// </summary>
    /// <param name="id">Marker ID</param>
    /// <param name="currentUsername">Current user's username</param>
    /// <param name="isAdmin">Whether the current user is an admin</param>
    Task DeleteAsync(int id, string currentUsername, bool isAdmin);

    /// <summary>
    /// Get list of available marker icons
    /// </summary>
    Task<List<string>> GetAvailableIconsAsync();
}



