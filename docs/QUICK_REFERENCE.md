# Quick Reference: What Works & What Doesn't

**Last Updated:** 2025-10-26
**Status:** Production-Ready (Core + Admin Complete)

A quick lookup table for determining feature availability in the .NET 9 implementation.

---

## ✅ WORKING - Game Client Features

| Feature | Endpoint | Status |
|---------|----------|--------|
| Version check | `POST /client/{token}/checkVersion` | ✅ WORKS |
| Grid location | `GET /client/{token}/locate` | ✅ WORKS |
| Grid sync | `POST /client/{token}/gridUpdate` | ✅ WORKS |
| Map upload | `POST /client/{token}/gridUpload` | ✅ WORKS |
| Character tracking | `POST /client/{token}/positionUpdate` | ✅ WORKS |
| Marker upload | `POST /client/{token}/markerBulkUpload` | ✅ WORKS |
| Marker delete | `POST /client/{token}/markerDelete` | ✅ WORKS |
| Marker update | `POST /client/{token}/markerUpdate` | ✅ WORKS |
| Harvest timers | `POST /client/{token}/markerReadyTime` | ✅ WORKS |

## ✅ WORKING - Map Viewing

| Feature | Endpoint | Status |
|---------|----------|--------|
| View characters | `GET /map/api/v1/characters` | ✅ WORKS |
| View markers | `GET /map/api/v1/markers` | ✅ WORKS |
| Map config | `GET /map/api/config` | ✅ WORKS |
| Map list | `GET /map/api/maps` | ✅ WORKS |
| Real-time updates | `GET /map/updates` | ✅ WORKS (SSE) |
| Tile images | `GET /map/grids/{mapid}/{zoom}/{x}_{y}.png` | ✅ WORKS |
| Delete tile (API) | `POST /map/api/admin/wipeTile` | ✅ WORKS |
| Shift coords (API) | `POST /map/api/admin/setCoords` | ✅ WORKS |
| Hide marker (API) | `POST /map/api/admin/hideMarker` | ✅ WORKS |
| Delete marker (API) | `POST /map/api/admin/deleteMarker` | ✅ WORKS |

## ✅ WORKING - User Features

| Feature | Endpoint | Status |
|---------|----------|--------|
| Login | `GET/POST /login` | ✅ WORKS (Blazor UI) |
| Logout | `GET /logout` | ✅ WORKS |
| Dashboard | `GET /` | ✅ WORKS (Blazor UI) |
| Generate token | `POST /generateToken` | ✅ WORKS |
| Change password | `GET/POST /password` | ✅ WORKS (Blazor UI) |

## ✅ WORKING - Admin Panel (NEW!)

| Feature | Endpoint/UI | Status |
|---------|-------------|--------|
| **Admin dashboard** | `GET /admin` | ✅ **WORKS (Blazor + MudBlazor)** |
| **Users Tab** | - | ✅ **WORKS** |
| - View users | `GET /admin/users` | ✅ WORKS |
| - Create user | `POST /admin/users` | ✅ WORKS (with dialog) |
| - Edit user permissions | `PUT /admin/users/{username}` | ✅ WORKS (with dialog) |
| - Change user password | `POST /admin/users/{username}/password` | ✅ WORKS (with dialog) |
| - Delete user | `DELETE /admin/users/{username}` | ✅ WORKS (with confirmation) |
| **Tokens Tab** | - | ✅ **WORKS** |
| - View all tokens | `GET /admin/tokens` | ✅ WORKS |
| - Revoke token | `DELETE /admin/tokens/{token}` | ✅ WORKS (with confirmation) |
| **System Tab** | - | ✅ **WORKS** |
| - View database stats | `GET /admin/system/stats` | ✅ WORKS |
| - Backup database | `POST /admin/system/backup` | ✅ WORKS |
| - Wipe all data | `POST /admin/system/wipe` | ✅ WORKS (with confirmation) |
| - Rebuild zoom tiles | `POST /admin/system/rebuild-tiles` | ⚠️ PLACEHOLDER (returns message) |
| - Clear tile cache | `POST /admin/system/clear-cache` | ✅ WORKS |
| **Database Tab** | - | ✅ **WORKS** |
| - Execute SQL queries | `POST /admin/database/query` | ✅ WORKS (SELECT only) |
| - Browse tables | `GET /admin/database/tables` | ✅ WORKS |
| - View table data | `GET /admin/database/table/{name}` | ✅ WORKS |
| - View schema | `GET /admin/database/schema` | ✅ WORKS |
| **Config Tab** | - | ⚠️ **PLACEHOLDER** ("coming soon") |

## ✅ WORKING - Core Services

| Feature | Implementation | Status |
|---------|---------------|--------|
| Character cleanup | BackgroundService (10s) | ✅ WORKS |
| Marker readiness | BackgroundService (30s) | ✅ WORKS |
| Image processing | ImageSharp BiLinear | ✅ WORKS |
| Zoom levels 1-6 | Automatic generation | ✅ WORKS |
| Map merging | GridService | ✅ WORKS |
| Session auth | ASP.NET Core Cookie Auth | ✅ WORKS |
| BCrypt passwords | BCrypt.Net | ✅ WORKS |
| Pub/sub updates | System.Threading.Channels | ✅ WORKS |
| SQLite database | EF Core | ✅ WORKS |
| .NET Aspire orchestration | AppHost | ✅ WORKS |
| Cross-service auth | Data Protection API | ✅ WORKS |

---

## ⚠️ PARTIALLY IMPLEMENTED

| Feature | Status | Notes |
|---------|--------|-------|
| Config management UI | ⚠️ PLACEHOLDER | Tab exists but no functionality |
| Rebuild zoom tiles | ⚠️ PLACEHOLDER | Endpoint exists but doesn't actually rebuild |

---

## ❌ NOT IMPLEMENTED

| Feature | Status | Workaround |
|---------|--------|-----------|
| Export data to ZIP | ❌ MISSING | Manual database file copy |
| Import/merge from ZIP | ❌ MISSING | Manual SQL import |
| Map management UI | ❌ MISSING | Direct SQL UPDATE on Maps table |
| Config management UI | ❌ MISSING | Direct SQL UPDATE on Config table |
| User activity logs | ❌ MISSING | Check database Sessions table |
| Performance metrics dashboard | ❌ MISSING | Use Aspire dashboard |

---

## Direct Database Access Guide

For features not yet in the UI, use direct SQL:

### Change Server Title
```sql
INSERT OR REPLACE INTO Config (Key, Value)
VALUES ('title', 'My HnH Server');
```

### Change URL Prefix
```sql
INSERT OR REPLACE INTO Config (Key, Value)
VALUES ('prefix', 'https://myserver.com');
```

### Toggle Map Hidden
```sql
UPDATE Maps SET Hidden = 1 WHERE Id = <mapid>;  -- Hide
UPDATE Maps SET Hidden = 0 WHERE Id = <mapid>;  -- Show
```

### Rename Map
```sql
UPDATE Maps SET Name = 'New Name' WHERE Id = <mapid>;
```

### Set Map Priority
```sql
UPDATE Maps SET Priority = 10 WHERE Id = <mapid>;
```

---

## Authorization Roles Reference

| Role | Constant | Purpose | Admin Panel Access |
|------|----------|---------|-------------------|
| admin | AUTH_ADMIN | Full admin access | ✅ Yes |
| map | AUTH_MAP | Can view map | ❌ No |
| markers | AUTH_MARKERS | Can see markers | ❌ No |
| point | AUTH_POINTER | Can see character positions | ❌ No |
| upload | AUTH_UPLOAD | Can generate tokens and upload data | ❌ No |
| writer | AUTH_WRITER | Can edit/delete tiles and markers | ❌ No |

**Note:** Admin panel requires `admin` role (case-insensitive).

---

## Common Tasks

### ✅ I want to... let game clients upload maps
**Status:** ✅ WORKS
**How:**
1. Login to admin panel
2. Go to Users tab
3. Create user with "upload" permission
4. Go to main dashboard
5. Generate token
6. Give token to game client

### ✅ I want to... view the map in a web browser
**Status:** ✅ WORKS
**How:** Login at `/login`, navigate to `/map`

### ✅ I want to... track character positions
**Status:** ✅ WORKS
**How:** Automatic via game client with upload permission

### ✅ I want to... create a new user account
**Status:** ✅ **NOW WORKS IN UI!**
**How:**
1. Login as admin
2. Navigate to `/admin`
3. Click "Users" tab
4. Click "Create User" button
5. Fill form with username, password, and permissions
6. Click "Create"

### ✅ I want to... change a user's permissions
**Status:** ✅ **NOW WORKS IN UI!**
**How:**
1. Login as admin
2. Navigate to `/admin`
3. Click "Users" tab
4. Click edit icon next to user
5. Check/uncheck permission boxes
6. Click "Save"

### ✅ I want to... view database contents
**Status:** ✅ **NOW WORKS IN UI!**
**How:**
1. Login as admin
2. Navigate to `/admin`
3. Click "Database" tab
4. Browse tables or execute SQL queries

### ⚠️ I want to... change site title
**Status:** ⚠️ NO UI YET
**Workaround:** Direct SQL UPDATE Config table (see above)

### ✅ I want to... backup my data
**Status:** ✅ **NOW WORKS IN UI!**
**How:**
1. Login as admin
2. Navigate to `/admin`
3. Click "System" tab
4. Click "Backup Database" button
5. Backup file created in `GridStorage` directory

**Additional backup (full):**
- Copy entire `map/` directory (includes database + all tiles)

### ⚠️ I want to... fix corrupted zoom tiles
**Status:** ⚠️ PLACEHOLDER IMPLEMENTATION
**Workaround:**
- Delete zoom tiles manually
- Restart server (auto-regenerates on access)
- OR use "Clear Tile Cache" button to delete all cached tiles

### ❌ I want to... merge data from another server
**Status:** ❌ NO IMPORT FEATURE
**Workaround:** Manual SQL data manipulation or wait for ZIP import feature

---

## Production Deployment Checklist

### ✅ Safe for Production
- [x] Game client communication
- [x] Map viewing
- [x] Real-time updates
- [x] Character tracking
- [x] Marker management
- [x] User login/logout
- [x] **Admin panel (user management)**
- [x] **Admin panel (token management)**
- [x] **Admin panel (database operations)**
- [x] **Admin panel (system operations)**

### ⚠️ Limitations
- [ ] Config management UI (workaround: direct SQL)
- [ ] Map management UI (workaround: direct SQL)
- [ ] Export/Import UI (workaround: manual file copy)
- [ ] Rebuild zoom tiles (workaround: manual deletion + restart)

### Recommendation
**For Game Clients:** ✅ Deploy immediately - fully functional
**For Server Administration:** ✅ **Deploy now - admin panel fully functional** (minor features via SQL)

---

## Technology Stack

| Component | Technology |
|-----------|-----------|
| Framework | .NET 9 |
| Web UI | Blazor Server + MudBlazor 6.11.2 |
| API | ASP.NET Core Minimal APIs |
| Orchestration | .NET Aspire |
| Database | SQLite + Entity Framework Core |
| Auth | Cookie Authentication + Data Protection API |
| Logging | Serilog |
| Real-time | Server-Sent Events (Channels) |

---

## Getting Help

| Topic | Resource |
|-------|----------|
| Comprehensive docs | [CLAUDE.md](../CLAUDE.md) |
| Quick start | [README.md](../README.md) |
| Feature details | [MIGRATION_REPORT.md](MIGRATION_REPORT.md) (may be outdated) |
| Source code | [../src/](../src/) |

---

## Recent Changes (2025-10-26)

### ✅ Admin Panel Implemented
- Blazor UI with MudBlazor
- User management (CRUD)
- Token management (view, revoke)
- Database viewer (SQL queries, schema)
- System operations (stats, backup, wipe, cache clear)

### ✅ Authentication Fixed
- Migrated to ASP.NET Core Cookie Authentication
- Implemented Data Protection API for cookie sharing
- Created AuthenticationDelegatingHandler for Web → API calls
- Fixed role detection case sensitivity issues

### ✅ All Admin Endpoints Working
- All `/admin/*` endpoints functional
- All `/admin/database/*` endpoints functional
- Proper authorization with role checks
- JSON response formats match DTOs

---

**Last Updated:** 2025-10-26
**Project Status:** Production-ready with fully functional admin panel!
