using HnHMapperServer.Core.Interfaces;
using Microsoft.AspNetCore.Http;

namespace HnHMapperServer.Services.Services;

/// <summary>
/// Provides access to the current tenant ID from the HTTP context
/// </summary>
public class TenantContextAccessor : ITenantContextAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantContextAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Gets the current tenant ID from the HTTP context
    /// </summary>
    /// <returns>Tenant ID if available, null otherwise</returns>
    public string? GetCurrentTenantId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return null;
        }

        return httpContext.Items["TenantId"] as string;
    }

    /// <summary>
    /// Gets the current tenant ID from the HTTP context, throws if not available
    /// </summary>
    /// <returns>Tenant ID</returns>
    /// <exception cref="InvalidOperationException">If tenant ID is not available in context</exception>
    public string GetRequiredTenantId()
    {
        var tenantId = GetCurrentTenantId();
        if (string.IsNullOrEmpty(tenantId))
        {
            throw new InvalidOperationException("Tenant ID is not available in the current context");
        }

        return tenantId;
    }
}
