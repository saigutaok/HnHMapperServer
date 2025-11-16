using HnHMapperServer.Core.Interfaces;
using HnHMapperServer.Core.Models;
using HnHMapperServer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HnHMapperServer.Infrastructure.Repositories;

public class ConfigRepository : IConfigRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContextAccessor _tenantContext;

    public ConfigRepository(ApplicationDbContext context, ITenantContextAccessor tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<Config> GetConfigAsync()
    {
        var config = new Config();

        var titleEntity = await _context.Config
            .FirstOrDefaultAsync(c => c.Key == "title");
        if (titleEntity != null)
            config.Title = titleEntity.Value;

        var prefixEntity = await _context.Config
            .FirstOrDefaultAsync(c => c.Key == "prefix");
        if (prefixEntity != null)
            config.Prefix = prefixEntity.Value;

        var defaultHideEntity = await _context.Config
            .FirstOrDefaultAsync(c => c.Key == "defaultHide");
        config.DefaultHide = defaultHideEntity != null;

        var mainMapEntity = await _context.Config
            .FirstOrDefaultAsync(c => c.Key == "mainMap");
        if (mainMapEntity != null && int.TryParse(mainMapEntity.Value, out var mainMapId))
            config.MainMapId = mainMapId;

        return config;
    }

    public async Task SaveConfigAsync(Config config)
    {
        await SetValueAsync("title", config.Title);
        await SetValueAsync("prefix", config.Prefix);

        if (config.DefaultHide)
        {
            await SetValueAsync("defaultHide", "true");
        }
        else
        {
            await DeleteValueAsync("defaultHide");
        }

        if (config.MainMapId.HasValue)
        {
            await SetValueAsync("mainMap", config.MainMapId.Value.ToString());
        }
        else
        {
            await DeleteValueAsync("mainMap");
        }
    }

    public async Task<string?> GetValueAsync(string key)
    {
        // IMPORTANT: FindAsync bypasses tenant filters - use explicit tenant check
        var currentTenantId = _tenantContext.GetRequiredTenantId();
        var entity = await _context.Config
            .FirstOrDefaultAsync(c => c.Key == key && c.TenantId == currentTenantId);
        return entity?.Value;
    }

    public async Task SetValueAsync(string key, string value)
    {
        // Config keys can be the same across tenants (e.g., "title", "prefix")
        // The PRIMARY KEY is (Key, TenantId), so each tenant can have their own config
        var currentTenantId = _tenantContext.GetRequiredTenantId();

        // Check if config key exists for current tenant (uses global query filter automatically)
        var existing = await _context.Config
            .FirstOrDefaultAsync(c => c.Key == key && c.TenantId == currentTenantId);

        if (existing != null)
        {
            // Update existing config for this tenant
            existing.Value = value;
            await _context.SaveChangesAsync();
        }
        else
        {
            // Insert new config for this tenant
            _context.Config.Add(new ConfigEntity
            {
                Key = key,
                Value = value,
                TenantId = currentTenantId
            });
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteValueAsync(string key)
    {
        // IMPORTANT: FindAsync bypasses tenant filters - use explicit tenant check
        var currentTenantId = _tenantContext.GetRequiredTenantId();
        var entity = await _context.Config
            .FirstOrDefaultAsync(c => c.Key == key && c.TenantId == currentTenantId);
        if (entity != null)
        {
            _context.Config.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
