using HnHMapperServer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HnHMapperServer.Infrastructure.Extensions;

/// <summary>
/// Extension methods for ApplicationDbContext
/// </summary>
public static class DbContextExtensions
{
    /// <summary>
    /// Creates a new DbContext instance that bypasses tenant query filters
    /// Use this for superadmin operations that need cross-tenant access
    /// </summary>
    /// <param name="context">The current DbContext</param>
    /// <returns>A new DbContext with query filters disabled</returns>
    /// <remarks>
    /// SECURITY WARNING: Only use this method for superadmin operations!
    /// Always verify the user has SuperAdmin role before calling this method.
    /// </remarks>
    public static IQueryable<T> IgnoreTenantFilter<T>(this IQueryable<T> query) where T : class
    {
        return query.IgnoreQueryFilters();
    }
}
