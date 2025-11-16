namespace HnHMapperServer.Core.DTOs;

/// <summary>
/// Complete character data for initial snapshot
/// </summary>
public class CharacterDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Map { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public string Type { get; set; } = string.Empty;
    public int Rotation { get; set; }
    public int Speed { get; set; }
}

/// <summary>
/// Character delta update containing changes since last broadcast
/// </summary>
public class CharacterDeltaDto
{
    /// <summary>
    /// Tenant ID for multi-tenancy isolation
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Characters that were added or updated
    /// </summary>
    public List<CharacterDto> Updates { get; set; } = new();

    /// <summary>
    /// Character IDs that were removed
    /// </summary>
    public List<int> Deletions { get; set; } = new();
}

