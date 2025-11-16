using HnHMapperServer.Core.DTOs;

namespace HnHMapperServer.Services.Interfaces;

/// <summary>
/// Service interface for ping operations with validation and rate limiting
/// </summary>
public interface IPingService
{
    /// <summary>
    /// Get all active (non-expired) pings for the current tenant
    /// </summary>
    Task<List<PingEventDto>> GetActiveForTenantAsync();

    /// <summary>
    /// Create a new ping with rate limiting (max 5 active pings per user)
    /// </summary>
    /// <param name="dto">Create ping DTO</param>
    /// <param name="currentUsername">Current user's username</param>
    /// <returns>Created ping event DTO</returns>
    /// <exception cref="InvalidOperationException">Thrown when user has reached ping limit</exception>
    Task<PingEventDto> CreateAsync(CreatePingDto dto, string currentUsername);

    /// <summary>
    /// Delete a specific ping by ID
    /// </summary>
    Task DeleteAsync(int id);

    /// <summary>
    /// Delete all expired pings and return them with tenant IDs (for background cleanup and SSE notification)
    /// </summary>
    Task<List<(int Id, string TenantId)>> DeleteExpiredAsync();
}
