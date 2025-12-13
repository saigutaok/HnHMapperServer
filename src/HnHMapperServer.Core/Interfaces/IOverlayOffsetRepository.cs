namespace HnHMapperServer.Core.Interfaces;

/// <summary>
/// Repository for managing overlay offset mappings.
/// Stores (currentMapId, overlayMapId) → (offsetX, offsetY) mappings per tenant.
/// </summary>
public interface IOverlayOffsetRepository
{
    /// <summary>
    /// Get overlay offset for a map pair. Returns null if not found.
    /// </summary>
    Task<(double offsetX, double offsetY)?> GetOffsetAsync(int currentMapId, int overlayMapId);

    /// <summary>
    /// Save or update overlay offset for a map pair.
    /// </summary>
    Task SaveOffsetAsync(int currentMapId, int overlayMapId, double offsetX, double offsetY);

    /// <summary>
    /// Delete overlay offset for a map pair.
    /// </summary>
    Task DeleteOffsetAsync(int currentMapId, int overlayMapId);
}
