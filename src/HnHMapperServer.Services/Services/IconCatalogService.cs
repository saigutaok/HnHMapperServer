using HnHMapperServer.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace HnHMapperServer.Services.Services;

/// <summary>
/// Service for managing and caching available marker icons from wwwroot
/// </summary>
public class IconCatalogService : IIconCatalogService
{
    private readonly ILogger<IconCatalogService> _logger;
    private readonly string _wwwrootPath;
    private List<string>? _cachedIcons;
    private DateTime? _lastCacheTime;
    private readonly SemaphoreSlim _cacheLock = new(1, 1);

    // Icon directories to scan relative to wwwroot
    private static readonly string[] IconDirectories =
    {
        "gfx/icons",
        "gfx/customicons",
        "gfx/terobjs/mm",
        "gfx/terobjs/mm/bushes",
        "gfx/terobjs/mm/trees",
        "gfx/invobjs",
        "gfx/kritter"
    };

    public IconCatalogService(string wwwrootPath, ILogger<IconCatalogService> logger)
    {
        _wwwrootPath = wwwrootPath;
        _logger = logger;
    }

    /// <summary>
    /// Get list of available marker icon names (cached in memory for 5 minutes)
    /// </summary>
    public async Task<List<string>> GetIconsAsync()
    {
        // Check if cache is valid (exists and less than 5 minutes old)
        if (_cachedIcons != null && _lastCacheTime.HasValue && 
            DateTime.UtcNow - _lastCacheTime.Value < TimeSpan.FromMinutes(5))
        {
            return _cachedIcons;
        }

        // Need to refresh cache
        await _cacheLock.WaitAsync();
        try
        {
            // Double-check after acquiring lock
            if (_cachedIcons != null && _lastCacheTime.HasValue && 
                DateTime.UtcNow - _lastCacheTime.Value < TimeSpan.FromMinutes(5))
            {
                return _cachedIcons;
            }

            await RefreshCacheInternalAsync();
            return _cachedIcons ?? new List<string>();
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    /// <summary>
    /// Refresh the icon cache (e.g., after adding new icons to wwwroot)
    /// </summary>
    public async Task RefreshCacheAsync()
    {
        await _cacheLock.WaitAsync();
        try
        {
            await RefreshCacheInternalAsync();
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    /// <summary>
    /// Internal method to refresh the cache (must be called with lock held)
    /// </summary>
    private Task RefreshCacheInternalAsync()
    {
        var icons = new List<string>();

        foreach (var iconDir in IconDirectories)
        {
            var fullPath = Path.Combine(_wwwrootPath, iconDir.Replace('/', Path.DirectorySeparatorChar));
            
            if (!Directory.Exists(fullPath))
            {
                _logger.LogWarning("Icon directory not found: {Path}", fullPath);
                continue;
            }

            try
            {
                // Scan for PNG files recursively
                var pngFiles = Directory.GetFiles(fullPath, "*.png", SearchOption.AllDirectories);
                
                foreach (var file in pngFiles)
                {
                    // Convert to relative path from wwwroot (using forward slashes for web)
                    var relativePath = Path.GetRelativePath(_wwwrootPath, file)
                        .Replace(Path.DirectorySeparatorChar, '/');
                    
                    icons.Add(relativePath);
                }

                _logger.LogDebug("Found {Count} icons in {Directory}", pngFiles.Length, iconDir);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scanning icon directory: {Path}", fullPath);
            }
        }

        // Sort alphabetically for consistent ordering
        icons.Sort();

        _cachedIcons = icons;
        _lastCacheTime = DateTime.UtcNow;

        _logger.LogInformation("Icon cache refreshed with {Count} icons", icons.Count);

        return Task.CompletedTask;
    }
}



