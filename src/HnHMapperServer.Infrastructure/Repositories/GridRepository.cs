using HnHMapperServer.Core.Interfaces;
using HnHMapperServer.Core.Models;
using HnHMapperServer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HnHMapperServer.Infrastructure.Repositories;

public class GridRepository : IGridRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContextAccessor _tenantContext;

    public GridRepository(ApplicationDbContext context, ITenantContextAccessor tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<GridData?> GetGridAsync(string gridId)
    {
        // Explicit tenant filtering for defense-in-depth
        var currentTenantId = _tenantContext.GetCurrentTenantId();

        var query = _context.Grids.AsNoTracking();

        // If tenant context is available, add explicit filter
        if (!string.IsNullOrEmpty(currentTenantId))
        {
            query = query.Where(g => g.TenantId == currentTenantId);
        }

        var entity = await query.FirstOrDefaultAsync(g => g.Id == gridId);
        return entity == null ? null : MapToDomain(entity);
    }

    public async Task SaveGridAsync(GridData gridData)
    {
        // Grid IDs are client-generated content hashes that can be the same across tenants
        // The PRIMARY KEY is (Id, TenantId), so each tenant can have their own copy
        var currentTenantId = _tenantContext.GetRequiredTenantId();

        // Check if grid exists for current tenant (uses global query filter automatically)
        var existing = await _context.Grids
            .FirstOrDefaultAsync(g => g.Id == gridData.Id && g.TenantId == currentTenantId);

        if (existing != null)
        {
            // Update existing grid for this tenant
            var entity = MapFromDomain(gridData);
            _context.Entry(existing).CurrentValues.SetValues(entity);
        }
        else
        {
            // Insert new grid for this tenant
            var entity = MapFromDomain(gridData);
            _context.Grids.Add(entity);
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteGridAsync(string gridId)
    {
        // Explicit tenant filtering for defense-in-depth
        var currentTenantId = _tenantContext.GetCurrentTenantId();

        var query = _context.Grids.AsQueryable();

        // If tenant context is available, add explicit filter
        if (!string.IsNullOrEmpty(currentTenantId))
        {
            query = query.Where(g => g.TenantId == currentTenantId);
        }

        var grid = await query.FirstOrDefaultAsync(g => g.Id == gridId);
        if (grid != null)
        {
            _context.Grids.Remove(grid);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<GridData>> GetAllGridsAsync()
    {
        // Explicit tenant filtering for defense-in-depth
        var currentTenantId = _tenantContext.GetCurrentTenantId();

        var query = _context.Grids.AsNoTracking();

        // If tenant context is available, add explicit filter
        if (!string.IsNullOrEmpty(currentTenantId))
        {
            query = query.Where(g => g.TenantId == currentTenantId);
        }

        var entities = await query.ToListAsync();
        return entities.Select(MapToDomain).ToList();
    }

    public async Task<List<GridData>> GetGridsByMapAsync(int mapId)
    {
        // Explicit tenant filtering for defense-in-depth
        var currentTenantId = _tenantContext.GetCurrentTenantId();

        var query = _context.Grids.AsNoTracking().Where(g => g.Map == mapId);

        // If tenant context is available, add explicit filter
        if (!string.IsNullOrEmpty(currentTenantId))
        {
            query = query.Where(g => g.TenantId == currentTenantId);
        }

        var entities = await query.ToListAsync();
        return entities.Select(MapToDomain).ToList();
    }

    public async Task DeleteGridsByMapAsync(int mapId)
    {
        // Explicit tenant filtering for defense-in-depth
        var currentTenantId = _tenantContext.GetCurrentTenantId();

        var query = _context.Grids.Where(g => g.Map == mapId);

        // If tenant context is available, add explicit filter
        if (!string.IsNullOrEmpty(currentTenantId))
        {
            query = query.Where(g => g.TenantId == currentTenantId);
        }

        var grids = await query.ToListAsync();

        _context.Grids.RemoveRange(grids);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> AnyGridsExistAsync()
    {
        // Explicit tenant filtering for defense-in-depth
        var currentTenantId = _tenantContext.GetCurrentTenantId();

        var query = _context.Grids.AsNoTracking();

        // If tenant context is available, add explicit filter
        if (!string.IsNullOrEmpty(currentTenantId))
        {
            query = query.Where(g => g.TenantId == currentTenantId);
        }

        return await query.AnyAsync();
    }

    private static GridData MapToDomain(GridDataEntity entity) => new GridData
    {
        Id = entity.Id,
        Coord = new Coord(entity.CoordX, entity.CoordY),
        NextUpdate = entity.NextUpdate,
        Map = entity.Map
    };

    private GridDataEntity MapFromDomain(GridData grid) => new GridDataEntity
    {
        Id = grid.Id,
        CoordX = grid.Coord.X,
        CoordY = grid.Coord.Y,
        NextUpdate = grid.NextUpdate,
        Map = grid.Map,
        TenantId = _tenantContext.GetRequiredTenantId()
    };
}
