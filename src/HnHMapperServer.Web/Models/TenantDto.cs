using System.Text.Json.Serialization;

namespace HnHMapperServer.Web.Models;

/// <summary>
/// DTO for tenant information
/// </summary>
public class TenantDto
{
    /// <summary>
    /// Tenant identifier (e.g., "warrior-shield-42")
    /// </summary>
    [JsonPropertyName("tenantId")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Tenant display name (e.g., "Warrior Shield 42")
    /// </summary>
    [JsonPropertyName("tenantName")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// User's role in this tenant (TenantAdmin, TenantUser, or SuperAdmin)
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// List of permissions the user has in this tenant
    /// (Map, Markers, Pointer, Upload, Writer)
    /// </summary>
    public List<string> Permissions { get; set; } = new();

    /// <summary>
    /// Current storage usage in MB
    /// </summary>
    public decimal StorageUsageMB { get; set; }

    /// <summary>
    /// Storage quota limit in MB
    /// </summary>
    public int StorageQuotaMB { get; set; }

    /// <summary>
    /// Whether the tenant is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Calculates storage usage percentage
    /// </summary>
    public int StorageUsagePercentage => StorageQuotaMB > 0
        ? (int)((StorageUsageMB / StorageQuotaMB) * 100)
        : 0;
}
