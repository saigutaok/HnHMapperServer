using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using System.Security.Claims;

namespace HnHMapperServer.Web.Services;

/// <summary>
/// Provides authentication state for Blazor Server from cookie-based authentication.
/// Uses a hybrid approach: HttpContext during initial render, then circuit-captured state.
/// </summary>
public class PersistentAuthenticationStateProvider : ServerAuthenticationStateProvider
{
    private readonly CircuitServicesAccessor _circuitServicesAccessor;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<PersistentAuthenticationStateProvider> _logger;

    public PersistentAuthenticationStateProvider(
        CircuitServicesAccessor circuitServicesAccessor,
        IHttpContextAccessor httpContextAccessor,
        ILogger<PersistentAuthenticationStateProvider> _logger)
    {
        _circuitServicesAccessor = circuitServicesAccessor;
        _httpContextAccessor = httpContextAccessor;
        this._logger = _logger;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        // Try to get user from the circuit (available after circuit starts)
        var user = _circuitServicesAccessor.User;

        // If circuit hasn't captured user yet, fall back to HttpContext (during initial render)
        if (user == null)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                user = httpContext.User;
                _logger.LogInformation("Using HttpContext.User during initial render - IsAuthenticated: {IsAuth}",
                    user?.Identity?.IsAuthenticated ?? false);
            }
        }
        else
        {
            _logger.LogInformation("Using circuit-captured user - IsAuthenticated: {IsAuth}",
                user.Identity?.IsAuthenticated ?? false);
        }

        // If still no user, return unauthenticated
        if (user == null)
        {
            _logger.LogWarning("No user available from either circuit or HttpContext");
            user = new ClaimsPrincipal(new ClaimsIdentity());
        }

        if (user.Identity?.IsAuthenticated == true)
        {
            _logger.LogInformation("Providing authenticated state for user: {Username}", user.Identity.Name);
        }
        else
        {
            _logger.LogInformation("Providing unauthenticated state");
        }

        return Task.FromResult(new AuthenticationState(user));
    }
}
