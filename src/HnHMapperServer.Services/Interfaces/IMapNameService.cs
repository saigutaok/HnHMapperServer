namespace HnHMapperServer.Services.Interfaces;

/// <summary>
/// Service for generating unique map names based on icon names.
/// </summary>
public interface IMapNameService
{
    /// <summary>
    /// Generates a unique map identifier in format: icon1-icon2-number (e.g., "arrow-wagon-4273")
    /// </summary>
    /// <param name="tenantId">The tenant ID to ensure uniqueness within tenant scope</param>
    /// <returns>Unique map identifier</returns>
    Task<string> GenerateUniqueIdentifierAsync(string tenantId);
}
