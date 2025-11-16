using HnHMapperServer.Core.Models;
using HnHMapperServer.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace HnHMapperServer.Services.Services;

/// <summary>
/// Singleton service that caches tiles in memory to prevent blocking database queries during SSE connections.
///
/// CRITICAL: GetAllTilesAsync() was called on every SSE connection without caching or AsNoTracking,
/// causing 3-5 second blocking queries that starved the thread pool and blocked SignalR heartbeats.
///
/// This service:
/// - Loads tiles once per tenant on first request (lazy initialization)
/// - Caches in memory per tenant for instant subsequent access
/// - Invalidates cache when tiles change (upload, delete, rebuild)
/// - Thread-safe using SemaphoreSlim
/// - TENANT-ISOLATED: Each tenant has a separate cache to prevent data leakage
/// </summary>
public class TileCacheService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<TileCacheService> _logger;
    private readonly SemaphoreSlim _cacheLock = new(1, 1);

    // Tenant-scoped cache: Dictionary<TenantId, (Tiles, LoadedAt)>
    private readonly Dictionary<string, (List<TileData> Tiles, DateTime LoadedAt)> _tenantCaches = new();

    public TileCacheService(IServiceScopeFactory serviceScopeFactory, ILogger<TileCacheService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// Get all tiles for a specific tenant (cached). First call loads from database, subsequent calls return cached data.
    /// </summary>
    /// <param name="tenantId">The tenant ID to load tiles for</param>
    public async Task<List<TileData>> GetAllTilesAsync(string tenantId)
    {
        if (string.IsNullOrEmpty(tenantId))
        {
            throw new ArgumentException("Tenant ID cannot be null or empty", nameof(tenantId));
        }

        // Fast path: Return cached data if available for this tenant
        if (_tenantCaches.TryGetValue(tenantId, out var cachedData))
        {
            _logger.LogDebug("Returning {Count} tiles from cache for tenant {TenantId} (loaded at {LoadedAt})",
                cachedData.Tiles.Count, tenantId, cachedData.LoadedAt);
            return cachedData.Tiles;
        }

        // Slow path: Load from database (once per tenant)
        await _cacheLock.WaitAsync();
        try
        {
            // Double-check after acquiring lock (another thread might have loaded)
            if (_tenantCaches.TryGetValue(tenantId, out cachedData))
            {
                _logger.LogDebug("Cache loaded by another thread for tenant {TenantId}, returning {Count} tiles",
                    tenantId, cachedData.Tiles.Count);
                return cachedData.Tiles;
            }

            _logger.LogInformation("Loading tiles from database for tenant {TenantId} (first request)...", tenantId);
            var startTime = DateTime.UtcNow;

            // Create scope to resolve scoped services
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                // Set tenant context in the scoped service provider
                var httpContextAccessor = scope.ServiceProvider.GetService<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
                if (httpContextAccessor?.HttpContext != null)
                {
                    httpContextAccessor.HttpContext.Items["TenantId"] = tenantId;
                }

                var tileRepository = scope.ServiceProvider.GetRequiredService<ITileRepository>();
                var tiles = await tileRepository.GetAllTilesAsync();

                // Store in tenant-specific cache
                var loadedAt = DateTime.UtcNow;
                _tenantCaches[tenantId] = (tiles, loadedAt);

                var loadTimeMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _logger.LogInformation("Loaded {Count} tiles for tenant {TenantId} into cache in {TimeMs}ms",
                    tiles.Count, tenantId, loadTimeMs);

                return tiles;
            }
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    /// <summary>
    /// Invalidate the cache for a specific tenant. Next GetAllTilesAsync() will reload from database.
    /// Call this when tiles are created, updated, or deleted.
    /// </summary>
    /// <param name="tenantId">The tenant ID to invalidate cache for, or null to invalidate all tenants</param>
    public async Task InvalidateCacheAsync(string? tenantId = null)
    {
        await _cacheLock.WaitAsync();
        try
        {
            if (tenantId == null)
            {
                // Invalidate all tenant caches
                if (_tenantCaches.Count > 0)
                {
                    _logger.LogInformation("Invalidating tile cache for all tenants ({Count} tenants cached)",
                        _tenantCaches.Count);
                    _tenantCaches.Clear();
                }
                else
                {
                    _logger.LogDebug("Cache invalidation requested but no tenants were cached");
                }
            }
            else
            {
                // Invalidate specific tenant cache
                if (_tenantCaches.Remove(tenantId, out var removedCache))
                {
                    _logger.LogInformation("Invalidated tile cache for tenant {TenantId} ({Count} tiles cached since {LoadedAt})",
                        tenantId, removedCache.Tiles.Count, removedCache.LoadedAt);
                }
                else
                {
                    _logger.LogDebug("Cache invalidation requested for tenant {TenantId} but it was not cached", tenantId);
                }
            }
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    /// <summary>
    /// Get cache statistics for monitoring/debugging.
    /// </summary>
    /// <param name="tenantId">The tenant ID to get stats for, or null for all tenants</param>
    public Dictionary<string, (bool IsCached, int TileCount, DateTime LoadedAt)> GetCacheStats(string? tenantId = null)
    {
        if (tenantId != null)
        {
            // Return stats for specific tenant
            if (_tenantCaches.TryGetValue(tenantId, out var cachedData))
            {
                return new Dictionary<string, (bool, int, DateTime)>
                {
                    [tenantId] = (true, cachedData.Tiles.Count, cachedData.LoadedAt)
                };
            }
            return new Dictionary<string, (bool, int, DateTime)>();
        }
        else
        {
            // Return stats for all tenants
            return _tenantCaches.ToDictionary(
                kvp => kvp.Key,
                kvp => (true, kvp.Value.Tiles.Count, kvp.Value.LoadedAt)
            );
        }
    }
}
