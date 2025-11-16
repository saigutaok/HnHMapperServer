using HnHMapperServer.Core.Constants;
using Microsoft.AspNetCore.Authorization;

namespace HnHMapperServer.Api.Authorization;

/// <summary>
/// Authorization handler for SuperadminOnlyRequirement.
/// Succeeds if the user has the SuperAdmin role (type-safe constant).
/// </summary>
public class SuperadminOnlyHandler : AuthorizationHandler<SuperadminOnlyRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SuperadminOnlyRequirement requirement)
    {
        if (context.User.IsInRole(AuthorizationConstants.Roles.SuperAdmin))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
