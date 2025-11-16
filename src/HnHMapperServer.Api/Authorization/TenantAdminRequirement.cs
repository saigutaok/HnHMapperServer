using Microsoft.AspNetCore.Authorization;

namespace HnHMapperServer.Api.Authorization;

/// <summary>
/// Authorization requirement that requires TenantAdmin role within the current tenant.
/// Superadmins automatically bypass this requirement.
/// </summary>
public class TenantAdminRequirement : IAuthorizationRequirement
{
}
