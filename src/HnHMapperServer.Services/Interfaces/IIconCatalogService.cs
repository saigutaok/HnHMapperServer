namespace HnHMapperServer.Services.Interfaces;

/// <summary>
/// Service for managing and caching available marker icons
/// </summary>
public interface IIconCatalogService
{
    /// <summary>
    /// Get list of available marker icon names (cached in memory)
    /// </summary>
    Task<List<string>> GetIconsAsync();

    /// <summary>
    /// Refresh the icon cache (e.g., after adding new icons to wwwroot)
    /// </summary>
    Task RefreshCacheAsync();
}



