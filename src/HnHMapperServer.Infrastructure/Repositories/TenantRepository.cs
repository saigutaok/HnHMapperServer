using HnHMapperServer.Core.Interfaces;
using HnHMapperServer.Core.Models;
using HnHMapperServer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HnHMapperServer.Infrastructure.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly ApplicationDbContext _context;

    public TenantRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TenantEntity?> GetByIdAsync(string id)
    {
        return await _context.Tenants
            .IgnoreQueryFilters() // Bypass tenant filter for tenant management
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<List<TenantEntity>> GetAllAsync()
    {
        return await _context.Tenants
            .IgnoreQueryFilters() // Bypass tenant filter to see all tenants
            .OrderBy(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<TenantEntity> CreateAsync(TenantEntity tenant)
    {
        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();
        return tenant;
    }

    public async Task UpdateAsync(TenantEntity tenant)
    {
        _context.Tenants.Update(tenant);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id)
    {
        var tenant = await GetByIdAsync(id);
        if (tenant != null)
        {
            _context.Tenants.Remove(tenant);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(string id)
    {
        return await _context.Tenants
            .IgnoreQueryFilters()
            .AnyAsync(t => t.Id == id);
    }
}
