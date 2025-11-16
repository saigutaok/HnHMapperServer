# HnH Mapper Server - .NET 9 Multi-Tenant Implementation

A complete .NET 9 implementation of the Haven & Hearth (HnH) Auto-Mapper Server with **enterprise multi-tenancy**, **full backward compatibility** with existing game clients, and a **modern Blazor admin interface**.

**Status:** ✅ **Production-Ready** (Core + Admin + Multi-Tenancy Complete)

**Current Branch:** `tenancy` (multi-tenant implementation)

---

## Features

### Core Features
✅ **Full backward compatibility** with existing HnH game clients
✅ **Enterprise Multi-Tenancy** with invitation-based registration
✅ **Blazor Web UI** with MudBlazor for modern admin experience
✅ **ASP.NET Core Identity** authentication with secure password hashing
✅ **.NET Aspire orchestration** for microservices management
✅ **SQLite database** with Entity Framework Core
✅ **Real-time updates** via Server-Sent Events (SSE)
✅ **Custom markers** with full CRUD API
✅ **Docker deployment** with automated CI/CD pipeline

### Multi-Tenancy Features
✅ **Complete data isolation** via EF Core global query filters
✅ **Invitation system** with admin approval workflow
✅ **Role hierarchy** (SuperAdmin, TenantAdmin, TenantUser)
✅ **Storage quotas** with real-time tracking
✅ **Audit logging** for all sensitive operations
✅ **Tenant-prefixed tokens** (`{tenantId}_{secret}`)

---

## Quick Start

### Prerequisites

- **.NET 9 SDK** ([Download](https://dotnet.microsoft.com/download/dotnet/9.0))
- (Optional) Docker for containerized deployment

### Running Locally

```bash
# Clone the repository
cd HnHMapperServer/src/HnHMapperServer.AppHost

# Run with Aspire (launches Web + API services)
dotnet run
```

The Aspire dashboard will open automatically, showing:
- **Web Service:** User dashboard and admin panel
- **API Service:** Game client APIs + admin APIs
- **Logs, Metrics, Health Checks**

Access the web UI at the URL shown in the Aspire dashboard (typically `http://localhost:XXXXX`).

### Default Admin Account

On first run, a default admin account is created:

**Username:** `admin`
**Password:** `admin123!`

⚠️ **IMPORTANT:** Change the default password immediately after first login!

---

## Architecture

This project uses **Clean Architecture** with **.NET Aspire** orchestration and **multi-tenant design**.

### Project Structure

```
HnHMapperServer/
├── src/
│   ├── HnHMapperServer.AppHost/         # .NET Aspire orchestration
│   ├── HnHMapperServer.ServiceDefaults/ # Aspire defaults (telemetry, health)
│   ├── HnHMapperServer.Core/            # Domain layer (entities, DTOs, enums)
│   ├── HnHMapperServer.Infrastructure/  # Data access (EF Core, SQLite)
│   ├── HnHMapperServer.Services/        # Business logic (multi-tenancy, quotas, audit)
│   ├── HnHMapperServer.Api/             # Game client APIs + Admin APIs
│   └── HnHMapperServer.Web/             # Blazor Web UI (MudBlazor)
├── deploy/                              # Docker deployment configs
│   ├── docker-compose.yml               # 4-service stack
│   ├── Caddyfile                        # Reverse proxy config
│   ├── VPS-SETUP.md                     # Deployment guide
│   └── SECURITY.md                      # Security best practices
├── map/                                 # Data storage (runtime)
│   ├── grids.db                         # SQLite database
│   ├── tenants/{tenantId}/grids/        # Tenant-isolated tile storage
│   └── DataProtection-Keys/             # Shared cookie encryption keys
├── CLAUDE.md                            # Comprehensive technical documentation
├── MULTI_TENANCY_DESIGN.md              # Multi-tenancy architecture (7,043 lines)
└── README.md                            # This file
```

### Technology Stack

| Component | Technology |
|-----------|------------|
| **Framework** | .NET 9 |
| **Web** | ASP.NET Core + Blazor Server |
| **UI Library** | MudBlazor 6.11.2 |
| **Orchestration** | .NET Aspire |
| **Database** | SQLite with EF Core |
| **Auth** | ASP.NET Core Identity |
| **Images** | SixLabors.ImageSharp |
| **Real-time** | Server-Sent Events (Channels) |
| **Logging** | Serilog |
| **Deployment** | Docker + GitHub Actions |

---

## What's Implemented

### ✅ Multi-Tenancy System (100%)

Complete enterprise multi-tenancy on the `tenancy` branch:

**Core Features:**
- **Tenant Isolation**: Complete data separation via EF Core global query filters
- **Invitation System**: Invite-code based registration with admin approval
- **Role Hierarchy**: SuperAdmin, TenantAdmin, TenantUser with granular permissions
- **Storage Quotas**: Per-tenant limits with real-time tracking and enforcement
- **Audit Logging**: Comprehensive audit trail for all operations
- **Token Format**: Tenant-prefixed tokens (`warrior-shield-42_a1b2c3d4...`)

**Database:**
- 5 new tables: Tenants, TenantUsers, TenantPermissions, TenantInvitations, AuditLogs
- All existing tables tenant-scoped with TenantId column
- ASP.NET Core Identity tables (AspNetUsers, AspNetRoles, etc.)

**UI:**
- Tenant admin panel with 6 tabs (Users, Tokens, Invitations, Pending Users, Audit Logs, Maps)
- Superadmin panel for managing all tenants
- Pending approval workflow for new users
- Tenant selector dropdown for multi-tenant users

### ✅ Game Client APIs (100%)

All 9 game client endpoints are **fully implemented**, **tenant-scoped**, and **backward compatible**:

| Endpoint | Purpose |
|----------|---------|
| `POST /client/{token}/checkVersion` | Version 4 validation |
| `GET /client/{token}/locate` | Grid location lookup |
| `POST /client/{token}/gridUpdate` | Map synchronization with merge logic |
| `POST /client/{token}/gridUpload` | Tile upload with winter season logic |
| `POST /client/{token}/positionUpdate` | Character tracking |
| `POST /client/{token}/markerBulkUpload` | Bulk marker creation |
| `POST /client/{token}/markerDelete` | Delete markers |
| `POST /client/{token}/markerUpdate` | Update marker status |
| `POST /client/{token}/markerReadyTime` | Harvest timer updates |

**Key Features:**
- Automatic tenant context resolution from token prefix
- Map merging with coordinate offset calculation
- Winter season logic (skip uploads when season==3)
- Storage quota enforcement (413 status when exceeded)

### ✅ Map Viewing & Real-time Updates (100%)

**Server-Sent Events (SSE):**
- Single persistent connection per client (replaced HTTP polling)
- Events: `charactersSnapshot`, `characterDelta`, `customMarkerCreated/Updated/Deleted`
- 250ms server-side coalescing with bounded channels (capacity 1024)
- Backpressure handling (DropOldest strategy)

**Map APIs:**
- Character positions (requires Map permission)
- Marker data (requires Markers permission)
- Tile image serving with 6 zoom levels (0-6)
- Map list and configuration

### ✅ Custom Markers (100%)

User-placed annotations with full CRUD API:

| Endpoint | Authorization |
|----------|---------------|
| `GET /map/api/v1/custom-markers` | Permission: Map |
| `GET /map/api/v1/custom-markers/{id}` | Permission: Map |
| `POST /map/api/v1/custom-markers` | Permission: Markers |
| `PUT /map/api/v1/custom-markers/{id}` | Creator or TenantAdmin |
| `DELETE /map/api/v1/custom-markers/{id}` | Creator or TenantAdmin |

**Features:**
- Icon whitelist validation
- HTML sanitization (strips all tags)
- Coordinate clamping (0-100 range)
- Real-time SSE updates on create/update/delete

### ✅ Admin APIs (100%)

**Tenant Admin Endpoints (10):**
- User management within tenant
- Token generation and revocation
- Invitation creation and management
- Pending user approval
- Tenant-scoped audit logs

**Superadmin Endpoints (13):**
- View and manage all tenants
- Manage unassigned users
- Adjust storage quotas
- View global audit logs
- System-wide operations

**Invitation Endpoints (4):**
- Create invitations with expiration
- Validate invite codes
- List active invitations
- Revoke invitations

### ✅ Background Services

| Service | Interval | Purpose |
|---------|----------|---------|
| `CharacterCleanupService` | 10s | Remove stale characters (timeout: 10s) |
| `MarkerReadinessService` | 30s | Update marker ready status |
| `MapCleanupService` | 10min | Delete empty maps older than 1 hour |
| `InvitationExpirationService` | 1 hour | Expire old invitations (7 day lifetime) |
| `TenantStorageVerificationService` | 6 hours | Verify storage quotas match actual disk usage |

---

## Multi-Tenancy

### Authentication Flow

1. User logs in at `/login`
2. If user belongs to multiple tenants → tenant selection page
3. Cookie created with tenant context in claims
4. All database queries automatically filtered by tenant via EF Core
5. Tenant context resolved from token prefix or user claims

### Role & Permission System

**Roles (TenantRole enum):**
- **SuperAdmin**: Full system access, manage all tenants
- **TenantAdmin**: Manage users within their tenant, create invitations
- **TenantUser**: Standard user with configurable permissions

**Permissions (Permission enum):**
- **Map**: View maps
- **Markers**: View and create markers
- **Pointer**: View character positions
- **Upload**: Upload tiles via game client
- **Writer**: Edit/delete tiles and markers

### Token Format

Multi-tenant tokens: `{tenantId}_{secret}`

Example: `warrior-shield-42_a1b2c3d4e5f6...`

- Tenant ID: Human-readable identifier (generated from adjective-noun-number pattern)
- Secret: SHA-256 hashed for storage
- Backward compatible with legacy tokens via migration

### Storage Quotas

- Per-tenant storage limits (default: 1024 MB)
- Real-time tracking on upload/delete
- Upload rejection when quota exceeded (HTTP 413)
- Background verification service (6 hour interval)
- UI gauge showing usage percentage

### Audit Logging

Comprehensive audit trail for:
- User creation/update/deletion
- Permission changes
- Token generation/revocation
- Invitation creation/usage
- Tenant configuration changes
- Map operations (delete, hide, coordinate shift)

Stored in `AuditLogs` table with:
- Timestamp, user, tenant, action, entity type/ID
- Old/new values for updates
- IP address and user agent

---

## Configuration

### appsettings.json

```json
{
  "GridStorage": "map",
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Cleanup": {
    "DeleteEmptyMapsAfterMinutes": 60,
    "MapCleanupIntervalSeconds": 600
  }
}
```

### Production Configuration (appsettings.Production.json)

Security-first defaults:
- `EnableCors`: false (CORS disabled by default)
- `EnableHttpsRedirect`: false (allows IP-only HTTP deployments)
- `SelfRegistration.Enabled`: false (invitation-only registration)
- `BootstrapAdmin.Enabled`: true (creates default admin user)

### Environment Variables

- `GridStorage`: Data directory (default: "map")
- `Cleanup:DeleteEmptyMapsAfterMinutes`: Empty map retention (default: 60)
- `Cleanup:MapCleanupIntervalSeconds`: Cleanup interval (default: 600)

### Runtime Configuration

Stored in `Config` table (tenant-scoped):
- `title`: Site title
- `prefix`: URL prefix for token display
- `defaultHide`: Default hidden status for new maps

---

## Database Schema

### Multi-Tenancy Tables

**Tenants:**
```sql
CREATE TABLE Tenants (
    Id TEXT PRIMARY KEY,              -- e.g., "warrior-shield-42"
    Name TEXT NOT NULL,
    StorageQuotaMB INTEGER NOT NULL DEFAULT 1024,
    CurrentStorageMB REAL NOT NULL DEFAULT 0,
    CreatedAt TEXT NOT NULL,
    IsActive INTEGER NOT NULL DEFAULT 1
);
```

**TenantUsers (many-to-many):**
```sql
CREATE TABLE TenantUsers (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    TenantId TEXT NOT NULL,
    UserId TEXT NOT NULL,             -- AspNetUsers.Id
    Role TEXT NOT NULL,               -- TenantAdmin or TenantUser
    JoinedAt TEXT NOT NULL,
    PendingApproval INTEGER NOT NULL DEFAULT 1
);
```

**TenantPermissions, TenantInvitations, AuditLogs**: See [CLAUDE.md](CLAUDE.md) for full schema.

### Core Tables (Tenant-Scoped)

All tables have `TenantId TEXT NOT NULL` column:

- **Maps**: Map metadata (id, name, hidden, priority, tenant)
- **Grids**: Grid tile metadata (coordinates, next update, tenant)
- **Tiles**: Tile images (zoom 0-6, cached PNGs, tenant)
- **Markers**: Map markers (position, harvest timers, tenant)
- **CustomMarkers**: User-placed annotations (tenant)
- **Tokens**: Authentication tokens (tenant-prefixed)
- **Characters**: Active character positions (tenant)
- **Config**: Runtime configuration (tenant-scoped)

**ASP.NET Identity Tables:**
- AspNetUsers, AspNetRoles, AspNetUserRoles, AspNetUserClaims, etc.

---

## Deployment

### Docker (Recommended)

**4-service stack** with automated CI/CD:

```bash
cd deploy
docker compose up -d
```

**Services:**
- `api`: Game client APIs + admin APIs (port 8080 internal)
- `web`: Blazor UI (port 8080 internal)
- `caddy`: Reverse proxy with path-based routing (port 80 external)
- `watchtower`: Auto-updates from GitHub Container Registry

**CI/CD Pipeline:**
Push to `main` branch → GitHub Actions builds images → Watchtower deploys within 60 seconds.

**Reverse Proxy Routing (Caddy):**
```
/client/*      → api service
/map/api/*     → api service
/map/grids/*   → api service
/map/updates   → api service (SSE)
/*             → web service (Blazor)
```

See [deploy/VPS-SETUP.md](deploy/VPS-SETUP.md) for complete deployment guide.

### Security

**Production Security Measures:**
- CORS disabled by default (prevents cross-origin attacks)
- HTTPS redirect opt-in (allows IP-only HTTP deployments)
- ASP.NET Identity password hashing (PBKDF2)
- SHA-256 token storage (tokens never stored plaintext)
- EF Core query filters (automatic tenant isolation)
- HTML sanitization for user input
- SQL injection protection (EF Core parameterized queries)

**Caddy Security Headers:**
```
X-Content-Type-Options: nosniff
X-Frame-Options: DENY
Referrer-Policy: no-referrer
Content-Security-Policy: (tuned for Blazor Server)
-Server (removes version disclosure)
```

See [deploy/SECURITY.md](deploy/SECURITY.md) for complete security checklist.

### Standalone (Development)

```bash
# Set shared data directory
export GridStorage=/srv/hnh-map

# Run with Aspire
cd src/HnHMapperServer.AppHost
dotnet run
```

---

## Migration from Go

This implementation maintains **100% API compatibility** with the original Go version while adding enterprise features:

| Go Feature | .NET Implementation |
|------------|---------------------|
| BoltDB | SQLite with EF Core |
| bcrypt | ASP.NET Core Identity (PBKDF2) |
| goroutines | Task, BackgroundService, Channels |
| channels | System.Threading.Channels |
| http.HandleFunc | ASP.NET Core Minimal APIs |
| html/template | Blazor Server + Razor |
| image processing | SixLabors.ImageSharp |
| Materialize CSS + Intercooler | MudBlazor (Material Design) |

**Improvements over Go version:**
- **Multi-tenancy** with complete data isolation
- **Invitation system** with approval workflow
- **Storage quotas** with real-time tracking
- **Audit logging** for compliance
- **Real-time SSE** (replaced HTTP polling)
- **Custom markers** with CRUD API
- **.NET Aspire orchestration**
- **Docker deployment** with CI/CD
- **Modern Blazor UI** (vs. server-rendered HTML)

---

## Known Limitations

1. **Rebuild Zoom Tiles**: Placeholder implementation (endpoint exists but doesn't rebuild)
2. **Export/Import**: Not implemented (manual database copy required)
3. **Map Management UI**: Limited (can't edit map properties from UI)

See [CLAUDE.md](CLAUDE.md) for comprehensive documentation and planned features.

---

## Troubleshooting

### "401 Unauthorized when accessing admin endpoints"

**Cause:** Cookie not forwarded or tenant context missing
**Solution:** Verify `AuthenticationDelegatingHandler` registered and Data Protection keys shared between Web/API services

### "Build fails with file locked errors"

**Cause:** Running services lock DLL files
**Solution:** Stop all processes:
```bash
taskkill /F /IM dotnet.exe  # Windows
pkill -9 dotnet              # Linux/Mac
```

### "User authenticated but has no roles"

**Cause:** TenantUser not approved or permissions not set
**Solution:** TenantAdmin must approve user in admin panel (Pending Users tab) and assign permissions

### "Storage quota exceeded"

**Cause:** Tenant has reached storage limit
**Solution:** Contact SuperAdmin to increase quota or delete old tiles

---

## Development

### Building

```bash
dotnet build
```

### Running Tests

```bash
dotnet test
```

### Code Structure

**Clean Architecture principles:**
- **Core**: Domain entities, DTOs, enums (no dependencies)
- **Infrastructure**: Database, repositories (depends on Core)
- **Services**: Business logic, multi-tenancy (depends on Core, Infrastructure)
- **Api/Web**: Presentation (depends on all layers)

### Adding a New Migration

```bash
cd src/HnHMapperServer.Infrastructure
dotnet ef migrations add MigrationName --startup-project ../HnHMapperServer.Api
dotnet ef database update --startup-project ../HnHMapperServer.Api
```

---

## Resources

### Documentation
- **[CLAUDE.md](CLAUDE.md)** - Comprehensive technical documentation (7,000+ lines)
- **[MULTI_TENANCY_DESIGN.md](MULTI_TENANCY_DESIGN.md)** - Multi-tenancy architecture details
- **[API_SPECIFICATION.md](API_SPECIFICATION.md)** - All endpoints with request/response schemas
- **[DATABASE_SCHEMA.md](DATABASE_SCHEMA.md)** - Complete schema with migrations
- **[deploy/VPS-SETUP.md](deploy/VPS-SETUP.md)** - Production deployment guide
- **[deploy/SECURITY.md](deploy/SECURITY.md)** - Security best practices

### External Links
- [.NET Aspire](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [MudBlazor Components](https://mudblazor.com/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [ASP.NET Core Identity](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/)
- [Blazor](https://learn.microsoft.com/en-us/aspnet/core/blazor/)

---

## Contributing

Contributions welcome! When contributing:

1. **Maintain backward compatibility** with game clients
2. **Test multi-tenant isolation** (verify tenant data doesn't leak)
3. **Update CLAUDE.md** to reflect changes
4. **Test authentication** across Web and API services
5. **Follow Clean Architecture** patterns
6. **Add audit logging** for sensitive operations
7. **Add tests** for new features

---

## License

Compatible with the original HnH Auto-Mapper Server license.

---

**Project Status:** Production-ready with complete multi-tenancy implementation. Full backward compatibility with existing game clients while adding enterprise features (tenant isolation, invitations, storage quotas, audit logging).

**Last Updated:** 2025-11-15
