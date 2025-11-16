using HnHMapperServer.Core.Enums;
using Microsoft.AspNetCore.Authorization;

namespace HnHMapperServer.Api.Authorization;

/// <summary>
/// Authorization requirement that requires a specific tenant permission.
/// Permissions: Map, Markers, Pointer, Upload, Writer (type-safe enum)
/// Superadmins automatically bypass this requirement.
/// </summary>
public class TenantPermissionRequirement : IAuthorizationRequirement
{
    public Permission Permission { get; }

    public TenantPermissionRequirement(Permission permission)
    {
        Permission = permission;
    }
}
