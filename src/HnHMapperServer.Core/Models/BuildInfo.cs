namespace HnHMapperServer.Core.Models;

/// <summary>
/// Represents build and version information for the application
/// </summary>
public class BuildInfo
{
    /// <summary>
    /// The service name (e.g., "api", "web")
    /// </summary>
    public string Service { get; set; } = string.Empty;

    /// <summary>
    /// The version string derived from git describe (e.g., "v1.4.2-3-gabc123")
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// The short Git commit SHA (e.g., "abc123")
    /// </summary>
    public string Commit { get; set; } = string.Empty;

    /// <summary>
    /// The build timestamp in UTC ISO format (e.g., "2025-11-02T10:22:31Z")
    /// </summary>
    public string BuildTimeUtc { get; set; } = string.Empty;

    /// <summary>
    /// Returns a formatted version string for display
    /// Format: "v1.4.2-3-gabc123 (sha:abc123, built:2025-11-02T10:22:31Z)"
    /// </summary>
    public string GetFormattedVersion()
    {
        if (string.IsNullOrEmpty(Version))
            return "dev";
        
        return $"{Version} (sha:{Commit}, built:{BuildTimeUtc})";
    }

    /// <summary>
    /// Returns a short version string for compact display
    /// Format: "v1.4.2-3-gabc123"
    /// </summary>
    public string GetShortVersion()
    {
        return string.IsNullOrEmpty(Version) ? "dev" : Version;
    }
}

