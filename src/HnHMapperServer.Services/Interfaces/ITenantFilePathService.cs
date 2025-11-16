namespace HnHMapperServer.Services.Interfaces;

/// <summary>
/// Service for constructing and validating tenant-specific file paths
/// </summary>
public interface ITenantFilePathService
{
    /// <summary>
    /// Gets the tenant-specific grid file path
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="gridId">Grid ID</param>
    /// <param name="gridStorage">Base grid storage path</param>
    /// <returns>Absolute file path</returns>
    string GetGridFilePath(string tenantId, string gridId, string gridStorage);

    /// <summary>
    /// Gets the tenant-specific tile file path
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="mapId">Map ID</param>
    /// <param name="zoom">Zoom level</param>
    /// <param name="coordName">Coordinate name (e.g., "1_2")</param>
    /// <param name="gridStorage">Base grid storage path</param>
    /// <returns>Absolute file path</returns>
    string GetTileFilePath(string tenantId, int mapId, int zoom, string coordName, string gridStorage);

    /// <summary>
    /// Gets the relative path for database storage
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="gridId">Grid ID</param>
    /// <returns>Relative path from gridStorage</returns>
    string GetGridRelativePath(string tenantId, string gridId);

    /// <summary>
    /// Gets the relative tile path for database storage
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="mapId">Map ID</param>
    /// <param name="zoom">Zoom level</param>
    /// <param name="coordName">Coordinate name</param>
    /// <returns>Relative path from gridStorage</returns>
    string GetTileRelativePath(string tenantId, int mapId, int zoom, string coordName);

    /// <summary>
    /// Gets the tenant directory path
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="gridStorage">Base grid storage path</param>
    /// <returns>Tenant directory path</returns>
    string GetTenantDirectory(string tenantId, string gridStorage);

    /// <summary>
    /// Validates that a path is within the tenant directory (prevents traversal attacks)
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="path">Path to validate</param>
    /// <param name="gridStorage">Base grid storage path</param>
    /// <exception cref="UnauthorizedAccessException">If path is outside tenant directory</exception>
    void ValidatePath(string tenantId, string path, string gridStorage);

    /// <summary>
    /// Ensures tenant directories exist
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="gridStorage">Base grid storage path</param>
    void EnsureTenantDirectoriesExist(string tenantId, string gridStorage);
}
