using HnHMapperServer.Core.DTOs;
using HnHMapperServer.Infrastructure.Data;

namespace HnHMapperServer.Services.Interfaces;

/// <summary>
/// Service for managing timers (marker timers and standalone timers).
/// Timers generate notifications when they expire.
/// </summary>
public interface ITimerService
{
    /// <summary>
    /// Create a new timer.
    /// </summary>
    /// <param name="dto">Timer data</param>
    /// <param name="userId">User ID of the creator</param>
    /// <param name="tenantId">Tenant ID</param>
    /// <returns>Created timer</returns>
    Task<TimerDto> CreateAsync(CreateTimerDto dto, string userId, string tenantId);

    /// <summary>
    /// Get a timer by ID.
    /// </summary>
    /// <param name="id">Timer ID</param>
    /// <returns>Timer or null if not found</returns>
    Task<TimerDto?> GetByIdAsync(int id);

    /// <summary>
    /// Get timers for a specific tenant.
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="includeCompleted">Whether to include completed timers</param>
    /// <param name="limit">Maximum number of results</param>
    /// <returns>List of timers</returns>
    Task<List<TimerDto>> GetTenantTimersAsync(
        string tenantId,
        bool includeCompleted = false,
        int limit = 100);

    /// <summary>
    /// Get timers for a specific user.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="includeCompleted">Whether to include completed timers</param>
    /// <param name="limit">Maximum number of results</param>
    /// <returns>List of timers</returns>
    Task<List<TimerDto>> GetUserTimersAsync(
        string userId,
        bool includeCompleted = false,
        int limit = 100);

    /// <summary>
    /// Get timers for a specific marker.
    /// </summary>
    /// <param name="markerId">Marker ID</param>
    /// <returns>List of timers for the marker</returns>
    Task<List<TimerDto>> GetMarkerTimersAsync(int markerId);

    /// <summary>
    /// Get timers for a specific custom marker.
    /// </summary>
    /// <param name="customMarkerId">Custom marker ID</param>
    /// <returns>List of timers for the custom marker</returns>
    Task<List<TimerDto>> GetCustomMarkerTimersAsync(int customMarkerId);

    /// <summary>
    /// Query timers with filtering.
    /// </summary>
    /// <param name="query">Query parameters</param>
    /// <returns>List of timers matching the query</returns>
    Task<List<TimerDto>> QueryAsync(TimerQuery query);

    /// <summary>
    /// Update a timer.
    /// </summary>
    /// <param name="id">Timer ID</param>
    /// <param name="dto">Update data</param>
    /// <param name="userId">User ID (for authorization check)</param>
    /// <returns>Updated timer or null if not found/unauthorized</returns>
    Task<TimerDto?> UpdateAsync(int id, UpdateTimerDto dto, string userId);

    /// <summary>
    /// Delete a timer.
    /// </summary>
    /// <param name="id">Timer ID</param>
    /// <param name="userId">User ID (for authorization check)</param>
    /// <returns>True if successful, false if not found or unauthorized</returns>
    Task<bool> DeleteAsync(int id, string userId);

    /// <summary>
    /// Complete a timer (mark as completed without waiting for expiry).
    /// </summary>
    /// <param name="id">Timer ID</param>
    /// <param name="userId">User ID (for authorization check)</param>
    /// <returns>True if successful, false if not found or unauthorized</returns>
    Task<bool> CompleteAsync(int id, string userId);

    /// <summary>
    /// Check for timers that need to be processed (expired or nearing expiry).
    /// Called by background service.
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <returns>Timers that need processing</returns>
    Task<List<TimerEntity>> GetTimersNeedingProcessingAsync(string tenantId);

    /// <summary>
    /// Mark a timer as having sent its notification.
    /// </summary>
    /// <param name="id">Timer ID</param>
    /// <returns>Task</returns>
    Task MarkNotificationSentAsync(int id);

    /// <summary>
    /// Mark a timer as having sent its pre-expiry warning.
    /// </summary>
    /// <param name="id">Timer ID</param>
    /// <returns>Task</returns>
    Task MarkPreExpiryWarningSentAsync(int id);

    /// <summary>
    /// Get timer history for a tenant.
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="limit">Maximum number of results</param>
    /// <returns>List of timer history entries</returns>
    Task<List<TimerHistoryDto>> GetHistoryAsync(string tenantId, int limit = 100);

    /// <summary>
    /// Get timer history for a specific marker.
    /// </summary>
    /// <param name="markerId">Marker ID</param>
    /// <returns>List of timer history entries</returns>
    Task<List<TimerHistoryDto>> GetMarkerHistoryAsync(int markerId);
}
