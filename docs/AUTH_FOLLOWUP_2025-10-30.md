## Authentication + Blazor UI follow-up (2025-10-30)

### Overview
This document captures what was broken, what we changed, and the final, standard configuration for authentication across `HnHMapperServer.Web` (Blazor Server UI) and `HnHMapperServer.Api` (Minimal API). It also includes dev-vs-prod guidance, diagnostics, and optional hardening steps.

### What we fixed
- Unified Data Protection keyring path so Web and API decrypt the same cookie.
- Standardized cookie authentication to the same scheme and cookie name in both services.
- Stopped 302 redirects for API/Admin endpoints; API now returns 401 for unauthorized requests.
- Corrected Blazor login behavior (form submission and prerendering) so login is responsive.
- Stopped Web from calling stale localhost ports; use service discovery or a known base URL.
- Added safe diagnostics to surface path/cookie mismatches during development.

### Final standard configuration (kept on purpose)
- Shared Data Protection with a single `ApplicationName` and a single keyring directory.
- Cookie Authentication with scheme `Cookies` and cookie name `HnH.Auth` in both services.
- Identity-based roles/claims and Blazor Server auth state capture for circuits.
- Web → API calls via `HttpClient` with a delegating handler that forwards `HnH.Auth` when services are on different origins.

## Configuration details

### Data Protection (Web and API)
Both services must point to the exact same keyring folder and use the same `ApplicationName`.

```csharp
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath))
    .SetApplicationName("HnHMapper");
```

Resolved defaults (when `GridStorage` is unset or relative):
- Resolved path: `<solution-root>/HnHMapperServer/src/map`
- Keyring: `<solution-root>/HnHMapperServer/src/map/DataProtection-Keys`

### Cookie authentication (API)
Use the default `Cookies` scheme and set the cookie to `HnH.Auth`. For API/Admin routes, return 401 instead of redirect.

```csharp
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.Cookie.Name = "HnH.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.Path = "/";
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;
    options.LoginPath = "/api/auth/login";
    options.LogoutPath = "/api/auth/logout";
    options.AccessDeniedPath = "/api/auth/login";
    options.Events.OnRedirectToLogin = ctx =>
    {
        if (ctx.Request.Path.StartsWithSegments("/api") || ctx.Request.Path.StartsWithSegments("/admin"))
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }
        ctx.Response.Redirect(ctx.RedirectUri);
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = ctx =>
    {
        if (ctx.Request.Path.StartsWithSegments("/api") || ctx.Request.Path.StartsWithSegments("/admin"))
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }
        ctx.Response.Redirect(ctx.RedirectUri);
        return Task.CompletedTask;
    };
});
```

### Cookie authentication (Web)
Mirror the same scheme and cookie settings.

```csharp
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "HnH.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.Path = "/";
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.SlidingExpiration = true;
        options.LoginPath = "/login";
        options.LogoutPath = "/login";
        options.AccessDeniedPath = "/login";
    });
```

### Blazor login form (Web)
- Disable prerendering on the login page so inputs don’t reset during the initial render.
- Use plain inputs named `user` and `pass` (or `username`/`password`) for POST compatibility.
- Ensure the login button is of type `submit`.

```razor
@attribute [RenderModeInteractiveServer(prerender: false)]

<form method="post" action="/api/login">
    <input class="mud-input" name="user" autocomplete="username" />
    <input class="mud-input" name="pass" type="password" autocomplete="current-password" />
    <MudButton ButtonType="ButtonType.Submit">Login</MudButton>
    <!-- Or <button type="submit">Login</button> -->
    <AntiforgeryToken />
    <!-- Antiforgery is globally enabled; the endpoint is DisableAntiforgery() -->
    <!-- We keep the token here for future CSRF enabling if desired. -->
  </form>
```

### Web → API HttpClient (service discovery, no auto-redirect)

```csharp
var apiBaseUrl = builder.Configuration["ApiBaseUrl"];
if (string.IsNullOrWhiteSpace(apiBaseUrl)) apiBaseUrl = "https://api";

builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    AllowAutoRedirect = false, // don’t follow 302s to login
    UseCookies = false         // we forward cookies manually
})
.AddHttpMessageHandler<AuthenticationDelegatingHandler>();
```

Delegating handler forwards `HnH.Auth` from the current request/circuit:

```csharp
// Pseudocode summary
var cookie = httpContext?.Request.Cookies["HnH.Auth"] ?? authStateCache.CookieValue;
if (!string.IsNullOrEmpty(cookie))
{
    request.Headers.Add("Cookie", $"HnH.Auth={cookie}");
}
```

## Development vs Production

### Recommended settings
- Dev
  - `SameSite = Lax`, `SecurePolicy = SameAsRequest` (local HTTPS + tooling friendly)
  - Verbose diagnostics (paths, cookie presence, auth failures)
- Prod
  - `SecurePolicy = Always` (HTTPS only), keep `SameSite = Lax` unless you truly need cross-site
  - Remove or minimize diagnostics; keep structured logging
  - Enable HSTS, strict TLS, and stable origins

### CORS
- The Web UI calls the API server-side, so CORS is typically unnecessary. If enabling browser-to-API calls, lock CORS down to known origins.

## Environment & discovery

- `GridStorage` and `ApiBaseUrl` can be provided via environment or config. If `GridStorage` is relative, it’s resolved against `<solution-root>/HnHMapperServer/src/` so both services share one path.
- Default API base for Aspire is `https://api`. When not using Aspire, set `ApiBaseUrl` explicitly (e.g., `https://localhost:7118`).

## Diagnostics & troubleshooting

Check these log lines on startup:
- Web: `GridStorage (raw|resolved) | DataProtection | API Base`
- API: `GridStorage (raw|resolved) | DataProtection`

If cookies don’t authenticate:
1) Verify Web and API report the same resolved `GridStorage` and `DataProtection` path.
2) Clear `HnH.Auth` in the browser and re-login.
3) Stop apps; delete the keyring folder, restart API then Web (regenerates keys).
4) Ensure the API emits 401 (not 302) for `/api/*` and `/admin/*` when unauthenticated.

## Production hardening checklist
- Force HTTPS in both services; set cookie `SecurePolicy = Always`.
- Remove development diagnostics and permissive CORS.
- Consider hosting API behind the Web origin (reverse proxy) so the cookie becomes first‑party without manual forwarding.
- Rotate Data Protection keys on a safe cadence if required by policy.
- Back up the SQLite DB and the keyring together.

## Optional: single-origin deployment

If you place API under Web (e.g., `/api`) using a reverse proxy (YARP, ingress), you can drop the delegating handler and rely on first‑party cookies. The rest of the configuration (same scheme, same cookie, same keyring) remains the same.

## Status
- Current implementation follows standard ASP.NET Core practices. No hacks were used.
- Dev-time diagnostics can be removed or gated behind `Development` environment when you’re satisfied.









