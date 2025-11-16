namespace HnHMapperServer.Core.Enums;

/// <summary>
/// Roles within a tenant organization.
/// Defines the level of administrative access a user has within their tenant.
/// </summary>
public enum TenantRole
{
    /// <summary>
    /// Standard user with assigned permissions.
    /// Can perform actions based on their granted permissions (Map, Markers, etc.)
    /// </summary>
    TenantUser,

    /// <summary>
    /// Administrator within the tenant.
    /// Can manage users, approve registrations, and assign permissions within their tenant.
    /// </summary>
    TenantAdmin
}
