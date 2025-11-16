namespace HnHMapperServer.Core.Interfaces;

/// <summary>
/// Provides access to the current tenant ID from the HTTP context
/// </summary>
public interface ITenantContextAccessor
{
    /// <summary>
    /// Gets the current tenant ID from the HTTP context
    /// </summary>
    /// <returns>Tenant ID if available, null otherwise</returns>
    string? GetCurrentTenantId();

    /// <summary>
    /// Gets the current tenant ID from the HTTP context, throws if not available
    /// </summary>
    /// <returns>Tenant ID</returns>
    /// <exception cref="InvalidOperationException">If tenant ID is not available in context</exception>
    string GetRequiredTenantId();
}
