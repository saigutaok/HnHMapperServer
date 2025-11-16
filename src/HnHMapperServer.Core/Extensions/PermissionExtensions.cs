using HnHMapperServer.Core.Enums;

namespace HnHMapperServer.Core.Extensions;

/// <summary>
/// Extension methods for Permission and TenantRole enums.
/// Provides conversion between enum values and string representations for database/claims.
/// </summary>
public static class AuthorizationExtensions
{
    /// <summary>
    /// Converts Permission enum to string for database storage and claim values.
    /// Example: Permission.Map → "Map"
    /// </summary>
    public static string ToClaimValue(this Permission permission)
    {
        return permission.ToString();
    }

    /// <summary>
    /// Converts string to Permission enum, case-insensitive.
    /// Example: "map" → Permission.Map
    /// </summary>
    public static Permission ToPermission(this string value)
    {
        return Enum.Parse<Permission>(value, ignoreCase: true);
    }

    /// <summary>
    /// Tries to convert string to Permission enum, returns false if invalid.
    /// </summary>
    public static bool TryToPermission(this string value, out Permission permission)
    {
        return Enum.TryParse(value, ignoreCase: true, out permission);
    }

    /// <summary>
    /// Converts TenantRole enum to string for database storage and claim values.
    /// Example: TenantRole.TenantAdmin → "TenantAdmin"
    /// </summary>
    public static string ToClaimValue(this TenantRole role)
    {
        return role.ToString();
    }

    /// <summary>
    /// Converts string to TenantRole enum, case-insensitive.
    /// Example: "tenantadmin" → TenantRole.TenantAdmin
    /// </summary>
    public static TenantRole ToTenantRole(this string value)
    {
        return Enum.Parse<TenantRole>(value, ignoreCase: true);
    }

    /// <summary>
    /// Tries to convert string to TenantRole enum, returns false if invalid.
    /// </summary>
    public static bool TryToTenantRole(this string value, out TenantRole role)
    {
        return Enum.TryParse(value, ignoreCase: true, out role);
    }
}
