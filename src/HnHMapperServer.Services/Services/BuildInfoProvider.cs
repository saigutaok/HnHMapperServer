using HnHMapperServer.Core.Models;
using HnHMapperServer.Services.Interfaces;
using System.Reflection;

namespace HnHMapperServer.Services.Services;

/// <summary>
/// Provides build and version information from environment variables or assembly metadata
/// </summary>
public class BuildInfoProvider : IBuildInfoProvider
{
    private readonly string _version;
    private readonly string _commit;
    private readonly string _buildTimeUtc;

    /// <summary>
    /// Initializes a new instance of BuildInfoProvider
    /// Reads version information from environment variables (BUILD_VERSION, BUILD_COMMIT, BUILD_DATE)
    /// Falls back to assembly informational version and current process start time if env vars are not set
    /// </summary>
    public BuildInfoProvider()
    {
        // Try to get version from environment variables (set by Docker build args)
        _version = Environment.GetEnvironmentVariable("BUILD_VERSION") ?? GetAssemblyVersion();
        _commit = Environment.GetEnvironmentVariable("BUILD_COMMIT") ?? "dev";
        _buildTimeUtc = Environment.GetEnvironmentVariable("BUILD_DATE") ?? GetProcessStartTime();
    }

    /// <summary>
    /// Gets build information for the specified service
    /// </summary>
    /// <param name="serviceName">The service name (e.g., "api", "web")</param>
    /// <returns>BuildInfo containing version, commit, and build time</returns>
    public BuildInfo Get(string serviceName)
    {
        return new BuildInfo
        {
            Service = serviceName,
            Version = _version,
            Commit = _commit,
            BuildTimeUtc = _buildTimeUtc
        };
    }

    /// <summary>
    /// Gets the assembly informational version as fallback
    /// This is typically set via AssemblyInformationalVersion attribute or MSBuild property
    /// </summary>
    private static string GetAssemblyVersion()
    {
        var assembly = Assembly.GetEntryAssembly();
        if (assembly == null)
            return "dev";

        var versionAttr = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (versionAttr != null && !string.IsNullOrEmpty(versionAttr.InformationalVersion))
        {
            return versionAttr.InformationalVersion;
        }

        // Fallback to assembly file version
        var version = assembly.GetName().Version;
        return version != null ? $"v{version.Major}.{version.Minor}.{version.Build}" : "dev";
    }

    /// <summary>
    /// Gets the current process start time in UTC ISO format as fallback build time
    /// </summary>
    private static string GetProcessStartTime()
    {
        return DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
    }
}

