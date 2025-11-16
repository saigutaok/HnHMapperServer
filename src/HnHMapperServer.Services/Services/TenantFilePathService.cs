using HnHMapperServer.Services.Interfaces;

namespace HnHMapperServer.Services.Services;

/// <summary>
/// Service for constructing and validating tenant-specific file paths
/// Format: gridStorage/tenants/{tenantId}/grids/{gridId}.png
///         gridStorage/tenants/{tenantId}/{mapId}/{zoom}/{coord}.png
/// </summary>
public class TenantFilePathService : ITenantFilePathService
{
    /// <summary>
    /// Gets the tenant-specific grid file path
    /// </summary>
    public string GetGridFilePath(string tenantId, string gridId, string gridStorage)
    {
        var relativePath = GetGridRelativePath(tenantId, gridId);
        var fullPath = Path.Combine(gridStorage, relativePath);
        ValidatePath(tenantId, fullPath, gridStorage);
        return fullPath;
    }

    /// <summary>
    /// Gets the tenant-specific tile file path
    /// </summary>
    public string GetTileFilePath(string tenantId, int mapId, int zoom, string coordName, string gridStorage)
    {
        var relativePath = GetTileRelativePath(tenantId, mapId, zoom, coordName);
        var fullPath = Path.Combine(gridStorage, relativePath);
        ValidatePath(tenantId, fullPath, gridStorage);
        return fullPath;
    }

    /// <summary>
    /// Gets the relative path for database storage
    /// </summary>
    public string GetGridRelativePath(string tenantId, string gridId)
    {
        return Path.Combine("tenants", tenantId, "grids", $"{gridId}.png");
    }

    /// <summary>
    /// Gets the relative tile path for database storage
    /// </summary>
    public string GetTileRelativePath(string tenantId, int mapId, int zoom, string coordName)
    {
        return Path.Combine("tenants", tenantId, mapId.ToString(), zoom.ToString(), $"{coordName}.png");
    }

    /// <summary>
    /// Gets the tenant directory path
    /// </summary>
    public string GetTenantDirectory(string tenantId, string gridStorage)
    {
        return Path.Combine(gridStorage, "tenants", tenantId);
    }

    /// <summary>
    /// Validates that a path is within the tenant directory (prevents traversal attacks)
    /// </summary>
    public void ValidatePath(string tenantId, string path, string gridStorage)
    {
        var tenantDir = GetTenantDirectory(tenantId, gridStorage);
        var fullTenantDir = Path.GetFullPath(tenantDir);
        var fullPath = Path.GetFullPath(path);

        if (!fullPath.StartsWith(fullTenantDir, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException(
                $"Path traversal detected: Path '{fullPath}' is outside tenant directory '{fullTenantDir}'");
        }
    }

    /// <summary>
    /// Ensures tenant directories exist
    /// </summary>
    public void EnsureTenantDirectoriesExist(string tenantId, string gridStorage)
    {
        var tenantDir = GetTenantDirectory(tenantId, gridStorage);
        var gridsDir = Path.Combine(tenantDir, "grids");

        Directory.CreateDirectory(tenantDir);
        Directory.CreateDirectory(gridsDir);
    }
}
