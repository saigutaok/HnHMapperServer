using HnHMapperServer.Core.Interfaces;
using HnHMapperServer.Core.Models;
using HnHMapperServer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HnHMapperServer.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for ping operations
/// </summary>
public class PingRepository : IPingRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContextAccessor _tenantContext;

    public PingRepository(ApplicationDbContext context, ITenantContextAccessor tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    /// <summary>
    /// Create a new ping
    /// </summary>
    public async Task<Ping> CreateAsync(Ping ping)
    {
        var entity = MapToEntity(ping);
        _context.Pings.Add(entity);
        await _context.SaveChangesAsync();

        // Update domain object with generated ID
        ping.Id = entity.Id;
        return ping;
    }

    /// <summary>
    /// Get all active (non-expired) pings for the current tenant
    /// </summary>
    public async Task<List<Ping>> GetActiveForTenantAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.Pings
            .AsNoTracking()
            .Where(p => p.ExpiresAt > now)
            .OrderBy(p => p.CreatedAt)
            .Select(p => new Ping
            {
                Id = p.Id,
                MapId = p.MapId,
                CoordX = p.CoordX,
                CoordY = p.CoordY,
                X = p.X,
                Y = p.Y,
                CreatedBy = p.CreatedBy,
                CreatedAt = p.CreatedAt,
                ExpiresAt = p.ExpiresAt
            })
            .ToListAsync();
    }

    /// <summary>
    /// Get count of active pings for a specific user
    /// </summary>
    public async Task<int> GetActiveCountByUserAsync(string username)
    {
        var now = DateTime.UtcNow;
        return await _context.Pings
            .AsNoTracking()
            .Where(p => p.CreatedBy == username && p.ExpiresAt > now)
            .CountAsync();
    }

    /// <summary>
    /// Delete a specific ping by ID
    /// </summary>
    public async Task DeleteAsync(int id)
    {
        var currentTenantId = _tenantContext.GetCurrentTenantId();
        if (string.IsNullOrEmpty(currentTenantId))
            return;

        var entity = await _context.Pings
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == currentTenantId);
        if (entity != null)
        {
            _context.Pings.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Delete all expired pings and return them (for SSE notification)
    /// NOTE: This method bypasses tenant filters to clean up all expired pings
    /// </summary>
    public async Task<List<(int Id, string TenantId)>> DeleteExpiredAsync()
    {
        var now = DateTime.UtcNow;

        // Must use IgnoreQueryFilters() to delete expired pings across all tenants
        var expiredPings = await _context.Pings
            .IgnoreQueryFilters()
            .Where(p => p.ExpiresAt <= now)
            .Select(p => new { p.Id, p.TenantId })
            .ToListAsync();

        var result = expiredPings.Select(p => (p.Id, p.TenantId)).ToList();

        if (expiredPings.Any())
        {
            // Delete by IDs (need to load entities first for deletion)
            var idsToDelete = expiredPings.Select(p => p.Id).ToList();
            var entitiesToDelete = await _context.Pings
                .IgnoreQueryFilters()
                .Where(p => idsToDelete.Contains(p.Id))
                .ToListAsync();

            _context.Pings.RemoveRange(entitiesToDelete);
            await _context.SaveChangesAsync();
        }

        return result;
    }

    /// <summary>
    /// Map entity to domain model
    /// </summary>
    private static Ping MapToDomain(PingEntity entity) => new()
    {
        Id = entity.Id,
        MapId = entity.MapId,
        CoordX = entity.CoordX,
        CoordY = entity.CoordY,
        X = entity.X,
        Y = entity.Y,
        CreatedBy = entity.CreatedBy,
        CreatedAt = entity.CreatedAt,
        ExpiresAt = entity.ExpiresAt
    };

    /// <summary>
    /// Map domain model to entity
    /// </summary>
    private PingEntity MapToEntity(Ping ping) => new()
    {
        Id = ping.Id,
        MapId = ping.MapId,
        CoordX = ping.CoordX,
        CoordY = ping.CoordY,
        X = ping.X,
        Y = ping.Y,
        CreatedBy = ping.CreatedBy,
        CreatedAt = ping.CreatedAt,
        ExpiresAt = ping.ExpiresAt,
        TenantId = _tenantContext.GetRequiredTenantId()
    };
}
