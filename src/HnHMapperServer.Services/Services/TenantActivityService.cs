using System.Collections.Concurrent;
using HnHMapperServer.Infrastructure.Data;
using HnHMapperServer.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HnHMapperServer.Services.Services;

/// <summary>
/// Service for tracking tenant activity with in-memory caching and periodic database flush.
/// Registered as singleton to maintain the activity cache across requests.
/// </summary>
public class TenantActivityService : ITenantActivityService
{
    private readonly ConcurrentDictionary<string, DateTime> _activityCache = new();
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TenantActivityService> _logger;

    public TenantActivityService(
        IServiceScopeFactory scopeFactory,
        ILogger<TenantActivityService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// Records activity for a tenant (stored in-memory, flushed periodically)
    /// </summary>
    public void RecordActivity(string tenantId)
    {
        if (string.IsNullOrEmpty(tenantId))
            return;

        _activityCache[tenantId] = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets all tenant activity times, merging in-memory cache with database values.
    /// Cache values take priority as they are more recent.
    /// </summary>
    public async Task<Dictionary<string, DateTime?>> GetAllLastActivitiesAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Get activity times from database
        var dbActivities = await db.Tenants
            .IgnoreQueryFilters()
            .ToDictionaryAsync(t => t.Id, t => t.LastActivityAt);

        // Merge with cache (cache takes priority as it's more recent)
        foreach (var (tenantId, cacheTime) in _activityCache)
        {
            if (!dbActivities.TryGetValue(tenantId, out var dbTime) ||
                dbTime == null ||
                cacheTime > dbTime)
            {
                dbActivities[tenantId] = cacheTime;
            }
        }

        return dbActivities;
    }

    /// <summary>
    /// Flushes cached activity times to the database.
    /// Called periodically by TenantActivityFlushService.
    /// </summary>
    public async Task FlushToDatabaseAsync()
    {
        if (_activityCache.IsEmpty)
            return;

        // Take a snapshot and clear the cache
        var snapshot = _activityCache.ToArray();
        _activityCache.Clear();

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        foreach (var (tenantId, activityTime) in snapshot)
        {
            try
            {
                await db.Tenants
                    .IgnoreQueryFilters()
                    .Where(t => t.Id == tenantId)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(t => t.LastActivityAt, activityTime));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to flush activity for tenant {TenantId}", tenantId);
                // Re-add to cache so it can be retried on next flush
                _activityCache.TryAdd(tenantId, activityTime);
            }
        }

        _logger.LogDebug("Flushed activity for {Count} tenants to database", snapshot.Length);
    }
}
