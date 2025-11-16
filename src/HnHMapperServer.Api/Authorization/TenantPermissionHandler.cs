using HnHMapperServer.Core.Constants;
using HnHMapperServer.Core.Extensions;
using Microsoft.AspNetCore.Authorization;

namespace HnHMapperServer.Api.Authorization;

/// <summary>
/// Authorization handler for TenantPermissionRequirement.
/// Succeeds if:
/// 1. User has SuperAdmin role (global bypass), OR
/// 2. User has the required permission in their TenantPermission claims
/// </summary>
public class TenantPermissionHandler : AuthorizationHandler<TenantPermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        TenantPermissionRequirement requirement)
    {
        // SuperAdmin bypasses permission checks
        if (context.User.IsInRole(AuthorizationConstants.Roles.SuperAdmin))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Check if user has specific permission (compare enum values)
        var permissionClaims = context.User.FindAll(AuthorizationConstants.ClaimTypes.TenantPermission);
        var requiredPermission = requirement.Permission.ToClaimValue();

        foreach (var claim in permissionClaims)
        {
            if (string.Equals(claim.Value, requiredPermission, StringComparison.OrdinalIgnoreCase))
            {
                context.Succeed(requirement);
                break;
            }
        }

        return Task.CompletedTask;
    }
}
