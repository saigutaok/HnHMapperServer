using Microsoft.AspNetCore.Authorization;

namespace HnHMapperServer.Api.Authorization;

/// <summary>
/// Authorization requirement that only allows Superadmin role.
/// Superadmins have global access to all tenants and can perform any operation.
/// </summary>
public class SuperadminOnlyRequirement : IAuthorizationRequirement
{
}
