namespace HnHMapperServer.Core.Enums;

/// <summary>
/// Tenant-level permissions for user authorization.
/// These permissions control what actions a user can perform within a tenant.
/// </summary>
public enum Permission
{
    /// <summary>
    /// View maps in the frontend
    /// </summary>
    Map,

    /// <summary>
    /// See markers on the map
    /// </summary>
    Markers,

    /// <summary>
    /// See character positions (live tracking)
    /// </summary>
    Pointer,

    /// <summary>
    /// Upload tiles via game client
    /// </summary>
    Upload,

    /// <summary>
    /// Edit/delete tiles and markers
    /// </summary>
    Writer
}
