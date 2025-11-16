# Identity-based Authentication (API-owned)

This system replaces all legacy auth (custom `Users`/`Sessions`, bespoke hashing) with ASP.NET Core Identity. The API owns login/logout and issues a shared cookie consumed by the Blazor Server UI.

## Components

- ASP.NET Core Identity with EF Core (SQLite)
- Cookie authentication (`HnH.Auth`) shared across API and Web via Data Protection
- Custom claims factory projects Identity roles to lowercase `auth` claims expected by UI (`admin`, `writer`, `point`, `map`, `markers`, `upload`)
- Game client token auth via `/client/{token}` path without changing the client

## Roles and Claims

- Identity roles: `Admin`, `Writer`, `Pointer`, `Map`, `Markers`, `Upload`
- UI claims: lowercase `auth` values mapped from roles by `AuthClaimsPrincipalFactory`

## Cookie Sharing

- Both API and Web use identical Data Protection key storage at `<GridStorage>/DataProtection-Keys`
- Cookie name: `HnH.Auth`, `SameSite=None`, `Secure=Always`, persistent 14 days

## API Endpoints

- `POST /api/auth/login` (JSON: `{ username, password }`) → sets cookie
- `POST /api/auth/logout` → clears cookie
- `GET /api/auth/me` → current user with roles and `auth` claims
- `GET /api/user/tokens` → list current user tokens
- `POST /api/user/tokens` → create a new upload-scoped token

## Admin Endpoints

- `GET /admin/users` → list users with roles
- `POST /admin/users` → create user (form: `username`, `password`, `roles` CSV)
- `PUT /admin/users/{username}/roles` → set roles (CSV)
- `DELETE /admin/users/{username}` → delete user (protects last admin)
- `POST /admin/users/{username}/password` → reset password
- `GET /admin/tokens` → list all tokens
- `POST /admin/tokens` → issue token for a user (form: `username`, `name`, `scopes`, optional `expiresAt`)
- `DELETE /admin/tokens/{token}` → revoke by Id or plaintext (hash-matched)

## Map/SSE Protection

- `/map/api/*` group requires `Map` role via policy `MapAccess`
- `/map/updates` SSE and `/map/grids/**` tiles also require `MapAccess`
- Per-operation checks still gate advanced features (e.g., `writer`, `markers`, `point`)

## Game Client Tokens (No Client Changes)

- `TokenEntity` stores SHA-256 hash of tokens linked to Identity user
- Client keeps sending requests to `/client/{token}/...` with the plaintext token in the path
- Scope check enforces `upload` for uploads; `LastUsedAt` updated on use

## Seeding

- On first run, roles are created
- Optional bootstrap admin via configuration:

```json
"BootstrapAdmin": {
  "Enabled": true,
  "Username": "admin",
  "Password": "admin"
}
```

## Security

- Password policy: min length 6 (no complexity requirements by default; adjust as needed)
- Cookie: HttpOnly, `SameSite=None`, `Secure=Always`, sliding expiration enabled
- Policies in API: `AdminOnly`, `UploaderOnly`, `MapAccess`, `MarkersAccess`, `PointerAccess`, `WriterAccess`, `AdminOrWriter`
- Antiforgery is enabled everywhere; login/logout endpoints explicitly disabled from antiforgery to allow programmatic access

## Blazor UI

- Uses cookie auth; `PersistentAuthenticationStateProvider` reads the cookie/circuit user
- `AuthenticationDelegatingHandler` forwards the `HnH.Auth` cookie to the API for server-side calls
- Login page posts credentials to API `/api/auth/login`












