using HnHMapperServer.Core.Interfaces;
using HnHMapperServer.Core.Models;
using HnHMapperServer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HnHMapperServer.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for custom marker operations
/// </summary>
public class CustomMarkerRepository : ICustomMarkerRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContextAccessor _tenantContext;

    public CustomMarkerRepository(ApplicationDbContext context, ITenantContextAccessor tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    /// <summary>
    /// Get a custom marker by ID
    /// </summary>
    public async Task<CustomMarker?> GetByIdAsync(int id)
    {
        // IMPORTANT: FindAsync bypasses tenant filters - use explicit tenant check
        var currentTenantId = _tenantContext.GetCurrentTenantId();
        if (string.IsNullOrEmpty(currentTenantId))
            return null;

        var entity = await _context.CustomMarkers
            .FirstOrDefaultAsync(m => m.Id == id && m.TenantId == currentTenantId);
        return entity == null ? null : MapToDomain(entity);
    }

    /// <summary>
    /// Get all custom markers for a specific map (includes hidden markers)
    /// </summary>
    public async Task<List<CustomMarker>> GetByMapIdAsync(int mapId)
    {
        return await _context.CustomMarkers
            .AsNoTracking()
            .Where(m => m.MapId == mapId)
            .OrderByDescending(m => m.PlacedAt)
            .Select(m => new CustomMarker
            {
                Id = m.Id,
                MapId = m.MapId,
                GridId = m.GridId,
                CoordX = m.CoordX,
                CoordY = m.CoordY,
                X = m.X,
                Y = m.Y,
                Title = m.Title,
                Description = m.Description,
                Icon = m.Icon,
                CreatedBy = m.CreatedBy,
                PlacedAt = m.PlacedAt,
                UpdatedAt = m.UpdatedAt,
                Hidden = m.Hidden
            })
            .ToListAsync();
    }

    /// <summary>
    /// Get all custom markers created by a specific user
    /// </summary>
    public async Task<List<CustomMarker>> GetByCreatorAsync(string username)
    {
        return await _context.CustomMarkers
            .AsNoTracking()
            .Where(m => m.CreatedBy == username)
            .OrderByDescending(m => m.PlacedAt)
            .Select(m => new CustomMarker
            {
                Id = m.Id,
                MapId = m.MapId,
                GridId = m.GridId,
                CoordX = m.CoordX,
                CoordY = m.CoordY,
                X = m.X,
                Y = m.Y,
                Title = m.Title,
                Description = m.Description,
                Icon = m.Icon,
                CreatedBy = m.CreatedBy,
                PlacedAt = m.PlacedAt,
                UpdatedAt = m.UpdatedAt,
                Hidden = m.Hidden
            })
            .ToListAsync();
    }

    /// <summary>
    /// Create a new custom marker
    /// </summary>
    public async Task<CustomMarker> CreateAsync(CustomMarker marker)
    {
        var entity = MapToEntity(marker);
        _context.CustomMarkers.Add(entity);
        await _context.SaveChangesAsync();
        
        // Update domain object with generated ID
        marker.Id = entity.Id;
        return marker;
    }

    /// <summary>
    /// Update an existing custom marker
    /// </summary>
    public async Task<CustomMarker> UpdateAsync(CustomMarker marker)
    {
        // IMPORTANT: FindAsync bypasses tenant filters - use explicit tenant check
        var currentTenantId = _tenantContext.GetRequiredTenantId();
        var trackedEntity = await _context.CustomMarkers
            .FirstOrDefaultAsync(m => m.Id == marker.Id && m.TenantId == currentTenantId);

        if (trackedEntity == null)
        {
            throw new KeyNotFoundException($"Custom marker with ID {marker.Id} not found.");
        }

        // Update properties
        trackedEntity.Title = marker.Title;
        trackedEntity.Description = marker.Description;
        trackedEntity.Icon = marker.Icon;
        trackedEntity.Hidden = marker.Hidden;
        trackedEntity.UpdatedAt = marker.UpdatedAt;
        // Note: PlacedAt, CreatedBy, MapId, GridId, CoordX, CoordY, X, Y are immutable

        await _context.SaveChangesAsync();
        return MapToDomain(trackedEntity);
    }

    /// <summary>
    /// Delete a custom marker
    /// </summary>
    public async Task DeleteAsync(int id)
    {
        // IMPORTANT: FindAsync bypasses tenant filters - use explicit tenant check
        var currentTenantId = _tenantContext.GetCurrentTenantId();
        if (string.IsNullOrEmpty(currentTenantId))
            return;

        var entity = await _context.CustomMarkers
            .FirstOrDefaultAsync(m => m.Id == id && m.TenantId == currentTenantId);
        if (entity != null)
        {
            _context.CustomMarkers.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Check if a custom marker exists
    /// </summary>
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.CustomMarkers
            .AsNoTracking()
            .AnyAsync(m => m.Id == id);
    }

    /// <summary>
    /// Map entity to domain model
    /// </summary>
    private static CustomMarker MapToDomain(CustomMarkerEntity entity) => new()
    {
        Id = entity.Id,
        MapId = entity.MapId,
        GridId = entity.GridId,
        CoordX = entity.CoordX,
        CoordY = entity.CoordY,
        X = entity.X,
        Y = entity.Y,
        Title = entity.Title,
        Description = entity.Description,
        Icon = entity.Icon,
        CreatedBy = entity.CreatedBy,
        PlacedAt = entity.PlacedAt,
        UpdatedAt = entity.UpdatedAt,
        Hidden = entity.Hidden
    };

    /// <summary>
    /// Map domain model to entity
    /// </summary>
    private CustomMarkerEntity MapToEntity(CustomMarker marker) => new()
    {
        Id = marker.Id,
        MapId = marker.MapId,
        GridId = marker.GridId,
        CoordX = marker.CoordX,
        CoordY = marker.CoordY,
        X = marker.X,
        Y = marker.Y,
        Title = marker.Title,
        Description = marker.Description,
        Icon = marker.Icon,
        CreatedBy = marker.CreatedBy,
        PlacedAt = marker.PlacedAt,
        UpdatedAt = marker.UpdatedAt,
        Hidden = marker.Hidden,
        TenantId = _tenantContext.GetRequiredTenantId()
    };
}

