using HnHMapperServer.Core.Interfaces;
using HnHMapperServer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HnHMapperServer.Infrastructure.Repositories;

public class OverlayOffsetRepository : IOverlayOffsetRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContextAccessor _tenantContext;

    public OverlayOffsetRepository(ApplicationDbContext context, ITenantContextAccessor tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<(double offsetX, double offsetY)?> GetOffsetAsync(int currentMapId, int overlayMapId)
    {
        var currentTenantId = _tenantContext.GetCurrentTenantId();
        if (string.IsNullOrEmpty(currentTenantId))
            return null;

        var entity = await _context.OverlayOffsets
            .FirstOrDefaultAsync(o =>
                o.TenantId == currentTenantId &&
                o.CurrentMapId == currentMapId &&
                o.OverlayMapId == overlayMapId);

        return entity != null
            ? (entity.OffsetX, entity.OffsetY)
            : null;
    }

    public async Task SaveOffsetAsync(int currentMapId, int overlayMapId, double offsetX, double offsetY)
    {
        var currentTenantId = _tenantContext.GetRequiredTenantId();

        var existing = await _context.OverlayOffsets
            .FirstOrDefaultAsync(o =>
                o.TenantId == currentTenantId &&
                o.CurrentMapId == currentMapId &&
                o.OverlayMapId == overlayMapId);

        if (existing != null)
        {
            existing.OffsetX = offsetX;
            existing.OffsetY = offsetY;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            _context.OverlayOffsets.Add(new OverlayOffsetEntity
            {
                TenantId = currentTenantId,
                CurrentMapId = currentMapId,
                OverlayMapId = overlayMapId,
                OffsetX = offsetX,
                OffsetY = offsetY,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteOffsetAsync(int currentMapId, int overlayMapId)
    {
        var currentTenantId = _tenantContext.GetCurrentTenantId();
        if (string.IsNullOrEmpty(currentTenantId))
            return;

        var entity = await _context.OverlayOffsets
            .FirstOrDefaultAsync(o =>
                o.TenantId == currentTenantId &&
                o.CurrentMapId == currentMapId &&
                o.OverlayMapId == overlayMapId);

        if (entity != null)
        {
            _context.OverlayOffsets.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
