using HnHMapperServer.Core.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace HnHMapperServer.Web.Services;

/// <summary>
/// Client service for retrieving version information from the API
/// </summary>
public class VersionClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<VersionClient> _logger;
    private BuildInfo? _cachedApiVersion;
    private DateTime _lastFetchTime = DateTime.MinValue;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Initializes a new instance of VersionClient
    /// </summary>
    /// <param name="httpClientFactory">Factory for creating HTTP clients</param>
    /// <param name="logger">Logger instance</param>
    public VersionClient(IHttpClientFactory httpClientFactory, ILogger<VersionClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// Gets the API version information
    /// Results are cached for 5 minutes to reduce API calls
    /// </summary>
    /// <returns>BuildInfo from API, or null if API is unreachable</returns>
    public async Task<BuildInfo?> GetApiVersionAsync()
    {
        // Return cached version if still valid
        if (_cachedApiVersion != null && (DateTime.UtcNow - _lastFetchTime) < _cacheExpiration)
        {
            return _cachedApiVersion;
        }

        try
        {
            // Create HTTP client configured with API base address and authentication
            var client = _httpClientFactory.CreateClient("API");
            
            // Call the public /version endpoint on the API service
            var response = await client.GetAsync("/version");
            
            if (response.IsSuccessStatusCode)
            {
                var buildInfo = await response.Content.ReadFromJsonAsync<BuildInfo>();
                if (buildInfo != null)
                {
                    _cachedApiVersion = buildInfo;
                    _lastFetchTime = DateTime.UtcNow;
                    return buildInfo;
                }
            }
            else
            {
                _logger.LogWarning("Failed to fetch API version: HTTP {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error fetching API version from /version endpoint");
        }

        // Return cached version even if expired, or null if never fetched
        return _cachedApiVersion;
    }
}

