using HnHMapperServer.Core.Enums;

namespace HnHMapperServer.Core.Models;

/// <summary>
/// Granular permissions for tenant users.
/// Stores permission as enum in code, string in database.
/// </summary>
public sealed class TenantPermissionEntity
{
    /// <summary>
    /// Auto-increment primary key
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to TenantUsers
    /// </summary>
    public int TenantUserId { get; set; }

    private string _permissionString = string.Empty;

    /// <summary>
    /// Permission enum value (Map, Markers, Pointer, Upload, Writer).
    /// Stored as string in database for compatibility.
    /// </summary>
    public Permission Permission
    {
        get => string.IsNullOrEmpty(_permissionString)
            ? Permission.Map
            : Enum.Parse<Permission>(_permissionString, ignoreCase: true);
        set => _permissionString = value.ToString();
    }

    /// <summary>
    /// Internal property for EF Core to map to database column.
    /// DO NOT use this directly in application code - use Permission property instead.
    /// </summary>
    public string PermissionString
    {
        get => _permissionString;
        set => _permissionString = value;
    }
}
