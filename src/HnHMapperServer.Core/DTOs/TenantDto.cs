namespace HnHMapperServer.Core.DTOs;

/// <summary>
/// DTO for tenant information
/// </summary>
public class TenantDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int StorageQuotaMB { get; set; }
    public double CurrentStorageMB { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public int UserCount { get; set; }
    public double StorageUsagePercent => StorageQuotaMB > 0 ? (CurrentStorageMB / StorageQuotaMB) * 100 : 0;
}

/// <summary>
/// DTO for creating a new tenant
/// </summary>
public class CreateTenantDto
{
    public int StorageQuotaMB { get; set; } = 1024;
}

/// <summary>
/// DTO for updating tenant settings
/// </summary>
public class UpdateTenantDto
{
    public string? Name { get; set; }
    public int? StorageQuotaMB { get; set; }
    public bool? IsActive { get; set; }
}
