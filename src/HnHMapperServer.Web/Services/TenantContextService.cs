using HnHMapperServer.Web.Models;
using System.Net.Http.Json;

namespace HnHMapperServer.Web.Services;

/// <summary>
/// State management service for current tenant context
/// Caches tenant list and provides tenant switching capabilities
/// </summary>
public class TenantContextService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<TenantContextService> _logger;

    private TenantDto? _currentTenant;
    private List<TenantDto>? _tenantCache;
    private DateTime? _cacheExpiry;

    // Cache for 5 minutes
    private readonly TimeSpan _cacheLifetime = TimeSpan.FromMinutes(5);

    public event EventHandler? TenantChanged;

    public TenantContextService(
        IHttpClientFactory httpClientFactory,
        ILogger<TenantContextService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// Gets the current tenant for the logged-in user
    /// </summary>
    public TenantDto? CurrentTenant => _currentTenant;

    /// <summary>
    /// Gets all tenants for the current user (cached)
    /// </summary>
    public async Task<List<TenantDto>> GetTenantsAsync(bool forceRefresh = false)
    {
        // Return cached tenants if valid
        if (!forceRefresh && _tenantCache != null && _cacheExpiry.HasValue && DateTime.UtcNow < _cacheExpiry.Value)
        {
            return _tenantCache;
        }

        try
        {
            var client = _httpClientFactory.CreateClient("API");
            var response = await client.GetAsync("/api/auth/tenants");

            if (response.IsSuccessStatusCode)
            {
                var tenants = await response.Content.ReadFromJsonAsync<List<TenantDto>>() ?? new List<TenantDto>();
                _tenantCache = tenants;
                _cacheExpiry = DateTime.UtcNow.Add(_cacheLifetime);

                // Set current tenant if we have exactly one tenant
                if (tenants.Count == 1 && _currentTenant == null)
                {
                    _currentTenant = tenants[0];
                }

                return tenants;
            }
            else
            {
                _logger.LogWarning("Failed to fetch tenants: {StatusCode}", response.StatusCode);
                return new List<TenantDto>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching tenants");
            return new List<TenantDto>();
        }
    }

    /// <summary>
    /// Sets the current tenant and raises TenantChanged event
    /// </summary>
    public void SetCurrentTenant(TenantDto? tenant)
    {
        var previousTenant = _currentTenant;
        _currentTenant = tenant;

        if (previousTenant?.Id != tenant?.Id)
        {
            TenantChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Clears the tenant cache (useful after logout or tenant changes)
    /// </summary>
    public void ClearCache()
    {
        _tenantCache = null;
        _cacheExpiry = null;
        _currentTenant = null;
    }
}
