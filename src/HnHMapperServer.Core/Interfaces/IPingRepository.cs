using HnHMapperServer.Core.Models;

namespace HnHMapperServer.Core.Interfaces;

/// <summary>
/// Repository interface for ping operations
/// </summary>
public interface IPingRepository
{
    /// <summary>
    /// Create a new ping
    /// </summary>
    Task<Ping> CreateAsync(Ping ping);

    /// <summary>
    /// Get all active (non-expired) pings for the current tenant
    /// </summary>
    Task<List<Ping>> GetActiveForTenantAsync();

    /// <summary>
    /// Get count of active pings for a specific user
    /// </summary>
    Task<int> GetActiveCountByUserAsync(string username);

    /// <summary>
    /// Delete a specific ping by ID
    /// </summary>
    Task DeleteAsync(int id);

    /// <summary>
    /// Delete all expired pings and return them (for SSE notification)
    /// </summary>
    Task<List<(int Id, string TenantId)>> DeleteExpiredAsync();
}
