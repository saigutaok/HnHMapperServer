namespace HnHMapperServer.Core.Models;

/// <summary>
/// Represents a character position update from game clients
/// </summary>
public class Character
{
    public string Name { get; set; } = string.Empty;
    public int Id { get; set; }
    public int Map { get; set; }
    public Position Position { get; set; } = new(0, 0);
    public string Type { get; set; } = string.Empty;
    public int Rotation { get; set; }
    public int Speed { get; set; }
    public DateTime Updated { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Previous position for speed/rotation calculation (server-side prediction)
    /// Used when clients don't send speed/rotation values
    /// </summary>
    public Position? PreviousPosition { get; set; }

    /// <summary>
    /// Previous update timestamp for speed/rotation calculation
    /// Used to calculate time delta for velocity computation
    /// </summary>
    public DateTime? PreviousUpdated { get; set; }

    /// <summary>
    /// Tenant ID for multi-tenancy isolation
    /// </summary>
    public string TenantId { get; set; } = string.Empty;
}
