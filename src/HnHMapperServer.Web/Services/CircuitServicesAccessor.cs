using Microsoft.AspNetCore.Components.Server.Circuits;
using System.Security.Claims;

namespace HnHMapperServer.Web.Services;

/// <summary>
/// Captures authentication state from HttpContext when a Blazor circuit is created.
/// Stores the ClaimsPrincipal and authentication cookie for the duration of the circuit
/// so they're available to background threads without requiring HttpContext.
/// </summary>
public class CircuitServicesAccessor : CircuitHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CircuitServicesAccessor> _logger;
    private readonly AuthenticationStateCache _authStateCache;

    public CircuitServicesAccessor(
        IHttpContextAccessor httpContextAccessor,
        ILogger<CircuitServicesAccessor> logger,
        AuthenticationStateCache authStateCache)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _authStateCache = authStateCache;
    }

    /// <summary>
    /// The user's claims principal captured from the initial HTTP request.
    /// This is populated when the circuit is created and remains for the circuit lifetime.
    /// </summary>
    public ClaimsPrincipal? User { get; private set; }

    public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        // Capture the user and authentication cookie from HttpContext when the circuit is first created
        // At this point, HttpContext IS available because we're in the initial HTTP request
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext != null)
        {
            User = httpContext.User;
            
            // Defensive: Ensure AuthenticationStateCache is not null before using it
            if (_authStateCache != null)
            {
                _authStateCache.User = User;

                // Capture the authentication cookie (new name with legacy fallback)
                var authCookie = httpContext.Request.Cookies["HnH.Auth"];
                if (string.IsNullOrEmpty(authCookie))
                {
                    authCookie = httpContext.Request.Cookies["HnHMapper.Auth"]; // legacy fallback
                }
                if (!string.IsNullOrEmpty(authCookie))
                {
                    _authStateCache.CookieValue = authCookie;
                    _logger.LogInformation("Captured authentication cookie for circuit (user: {Username})", User?.Identity?.Name ?? "unknown");
                }
                else
                {
                    // Clear any stale cookie value
                    _authStateCache.CookieValue = null;
                    _logger.LogWarning("No authentication cookie found in request when creating circuit (user authenticated: {IsAuth})",
                        User?.Identity?.IsAuthenticated ?? false);
                }
            }
            else
            {
                _logger.LogError("AuthenticationStateCache is null when creating circuit");
            }

            if (User?.Identity?.IsAuthenticated == true)
            {
                _logger.LogInformation("Circuit created for authenticated user: {Username}", User.Identity.Name);
            }
            else
            {
                _logger.LogInformation("Circuit created for unauthenticated user");
            }
        }
        else
        {
            _logger.LogWarning("HttpContext was null when creating circuit");
            User = new ClaimsPrincipal(new ClaimsIdentity());
            if (_authStateCache != null)
            {
                _authStateCache.User = User;
                _authStateCache.CookieValue = null;
            }
        }

        return base.OnConnectionUpAsync(circuit, cancellationToken);
    }

    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        if (User?.Identity?.IsAuthenticated == true)
        {
            _logger.LogInformation("Circuit closed for user: {Username}", User.Identity.Name);
        }

        // Clear the user and cookie when circuit closes
        User = null;
        if (_authStateCache != null)
        {
            _authStateCache.User = null;
            _authStateCache.CookieValue = null;
        }

        return base.OnCircuitClosedAsync(circuit, cancellationToken);
    }
}
