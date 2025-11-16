namespace HnHMapperServer.Core.Constants;

/// <summary>
/// Authorization-related constants for claims, policies, and roles.
/// Using constants ensures type safety and prevents typos in authorization code.
/// </summary>
public static class AuthorizationConstants
{
    /// <summary>
    /// Claim type constants for use in ClaimsPrincipal
    /// </summary>
    public static class ClaimTypes
    {
        /// <summary>
        /// Claim type for tenant-specific permissions (Map, Markers, Pointer, Upload, Writer)
        /// </summary>
        public const string TenantPermission = "TenantPermission";

        /// <summary>
        /// Claim type for tenant identifier (e.g., "default-tenant-1")
        /// </summary>
        public const string TenantId = "TenantId";

        /// <summary>
        /// Claim type for role within tenant (TenantAdmin or TenantUser)
        /// </summary>
        public const string TenantRole = "TenantRole";
    }

    /// <summary>
    /// Authorization policy names for use in [Authorize] attributes and RequireAuthorization()
    /// </summary>
    public static class Policies
    {
        /// <summary>
        /// Policy requiring SuperAdmin global role (access to all tenants)
        /// </summary>
        public const string SuperAdminOnly = "SuperadminOnly";

        /// <summary>
        /// Policy requiring TenantAdmin role within current tenant
        /// </summary>
        public const string TenantAdmin = "TenantAdmin";

        /// <summary>
        /// Policy requiring Map permission within current tenant
        /// </summary>
        public const string TenantMapAccess = "TenantMapAccess";

        /// <summary>
        /// Policy requiring Markers permission within current tenant
        /// </summary>
        public const string TenantMarkersAccess = "TenantMarkersAccess";

        /// <summary>
        /// Policy requiring Pointer permission within current tenant
        /// </summary>
        public const string TenantPointerAccess = "TenantPointerAccess";

        /// <summary>
        /// Policy requiring Upload permission within current tenant
        /// </summary>
        public const string TenantUpload = "TenantUpload";

        /// <summary>
        /// Policy requiring Writer permission within current tenant
        /// </summary>
        public const string TenantWriter = "TenantWriter";
    }

    /// <summary>
    /// Role names for use in IsInRole() checks
    /// </summary>
    public static class Roles
    {
        /// <summary>
        /// Global SuperAdmin role (access to all tenants and administrative functions)
        /// </summary>
        public const string SuperAdmin = "SuperAdmin";
    }
}
