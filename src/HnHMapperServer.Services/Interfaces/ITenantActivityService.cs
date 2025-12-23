namespace HnHMapperServer.Services.Interfaces;

/// <summary>
/// Service for tracking tenant activity (grid uploads, position updates)
/// Uses in-memory caching with periodic database flush for performance
/// </summary>
public interface ITenantActivityService
{
    /// <summary>
    /// Records activity for a tenant (in-memory, will be flushed periodically)
    /// </summary>
    void RecordActivity(string tenantId);

    /// <summary>
    /// Gets all tenant activity times (merges in-memory cache with database values)
    /// </summary>
    Task<Dictionary<string, DateTime?>> GetAllLastActivitiesAsync();

    /// <summary>
    /// Flushes cached activity times to the database
    /// </summary>
    Task FlushToDatabaseAsync();
}
