using HnHMapperServer.Core.Models;

namespace HnHMapperServer.Services.Interfaces;

/// <summary>
/// Provides access to build and version information
/// </summary>
public interface IBuildInfoProvider
{
    /// <summary>
    /// Gets build information for the specified service
    /// </summary>
    /// <param name="serviceName">The service name (e.g., "api", "web")</param>
    /// <returns>BuildInfo containing version, commit, and build time</returns>
    BuildInfo Get(string serviceName);
}

