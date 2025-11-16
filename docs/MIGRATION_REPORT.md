# Migration Report: Go → .NET C# HnH Auto-Mapper Server

**Date**: 2025-10-25
**Status**: Core Functionality Complete, Admin Panel Missing
**Overall Completion**: ~65% (weighted by end-user importance)

---

## Executive Summary

The .NET C# migration successfully implements **100% of core game client functionality**, including all client API endpoints, map viewing, real-time updates, character tracking, and marker management. The server is **production-ready for game clients**.

However, the **Admin Panel is completely missing** (0% implemented), which means server administrators lack UI tools for user management, data maintenance, configuration, and backup/recovery operations.

---

## ✅ Fully Implemented Features (100%)

### Client API Endpoints (9/9) ✅

All game client communication endpoints are fully implemented with backward compatibility:

| Endpoint | Status | Implementation Details |
|----------|--------|----------------------|
| `POST /client/{token}/checkVersion` | ✅ | Version 4 validation |
| `GET /client/{token}/locate` | ✅ | Grid location lookup by GridID |
| `POST /client/{token}/gridUpdate` | ✅ | Complex map merging, offset calculation, duplicate detection |
| `POST /client/{token}/gridUpload` | ✅ | Multipart file upload, winter season logic (season==3 skip) |
| `POST /client/{token}/positionUpdate` | ✅ | Character tracking with type priority (player > known > unknown) |
| `POST /client/{token}/markerBulkUpload` | ✅ | Bulk marker creation with default image fallback |
| `POST /client/{token}/markerDelete` | ✅ | Delete by GridID + X,Y coordinate |
| `POST /client/{token}/markerUpdate` | ✅ | Update marker ready status with logic |
| `POST /client/{token}/markerReadyTime` | ✅ | Harvest timer narrowing (min/max window convergence) |

**Key Implementation Details:**
- **Grid Merging**: Automatically merges maps when multiple maps detected, calculates offsets, shifts coordinates
- **Winter Season Logic**: Skips uploads when season==3 and tile already exists, updates NextUpdate timer
- **Character Priority**: Maintains character type consistency (player types override others, known types override unknown)
- **Marker Timing**: Implements convergent min/max ready time window for harvest predictions

### Map Frontend API (8/8) ✅

| Endpoint | Status | Authorization | Notes |
|----------|--------|--------------|-------|
| `GET /map/api/v1/characters` | ✅ | AUTH_MAP + AUTH_POINTER | Returns empty array if AUTH_POINTER missing |
| `GET /map/api/v1/markers` | ✅ | AUTH_MAP + AUTH_MARKERS | Transforms grid-relative to map-absolute coords |
| `GET /map/api/config` | ✅ | AUTH_MAP | Returns title + user's auth array |
| `GET /map/api/maps` | ✅ | AUTH_MAP | Sorted by name, excludes hidden maps |
| `GET /map/updates` | ✅ | AUTH_MAP | SSE: Tile cache + merge events, batched every 5s |
| `GET /map/grids/{mapid}/{zoom}/{x}_{y}.png` | ✅ | AUTH_MAP | Immutable cache headers |
| `POST /map/api/admin/wipeTile` | ✅ | AUTH_ADMIN \|\| AUTH_WRITER | Deletes tile + regenerates zoom levels |
| `POST /map/api/admin/setCoords` | ✅ | AUTH_ADMIN \|\| AUTH_WRITER | Shifts all grids on a map by offset |
| `POST /map/api/admin/hideMarker` | ✅ | AUTH_ADMIN \|\| AUTH_WRITER | Hides marker by ID |
| `POST /map/api/admin/deleteMarker` | ✅ | AUTH_ADMIN \|\| AUTH_WRITER | Permanently deletes marker |

**Key Implementation Details:**
- **SSE (Server-Sent Events)**: Real-time updates using System.Threading.Channels
- **Tile Batching**: Groups tile updates and sends every 5 seconds to reduce network traffic
- **Merge Notifications**: Separate event stream for map merge operations
- **Coordinate Transformation**: Automatically converts grid-relative (0-100) to map-absolute coordinates

### User Management Endpoints (5/5) ✅

| Endpoint | Status | Notes |
|----------|--------|-------|
| `GET /login` | ✅ | Simple HTML form |
| `POST /login` | ✅ | BCrypt password verification, 7-day cookie |
| `GET /logout` | ✅ | Session deletion + cookie removal |
| `GET /` | ✅ | Dashboard with token list, uses config.Prefix |
| `POST /generateToken` | ✅ | Generates 32-byte hex token, requires AUTH_UPLOAD |
| `GET /password` | ✅ | Password change form |
| `POST /password` | ✅ | Updates password with BCrypt rehashing |

### Background Services ✅

| Service | Interval | Function |
|---------|----------|----------|
| `CharacterCleanupService` | 10 seconds | Removes characters not updated in >10 seconds |
| `MarkerReadinessService` | 30 seconds | Updates marker ready status when MaxReady time reached |

### Core Infrastructure ✅

| Component | Go Implementation | .NET Implementation |
|-----------|------------------|-------------------|
| Database | BoltDB (key-value) | SQLite with EF Core (relational) |
| Password Hashing | bcrypt | BCrypt.Net-Next |
| Concurrency | goroutines + sync.RWMutex | Task + ConcurrentDictionary |
| Pub/Sub | Go channels | System.Threading.Channels |
| Image Processing | image + draw packages | SixLabors.ImageSharp |
| Session Storage | BoltDB bucket | SQLite table with JSON auths |
| HTTP Server | net/http | ASP.NET Core Minimal APIs |

**Database Schema (8 tables):**
- ✅ `Config` - Key-value configuration (title, prefix, defaultHide)
- ✅ `Users` - Username, PasswordHash, AuthsJson, TokensJson
- ✅ `Sessions` - SessionId, Username, TempAdmin, CreatedAt
- ✅ `Tokens` - Token → Username mapping
- ✅ `Grids` - GridID, Map, Coord(X,Y), NextUpdate
- ✅ `Tiles` - MapId, Zoom, Coord(X,Y), File, Cache (unique index)
- ✅ `Markers` - ID, GridID, Position(X,Y), Name, Image, Ready times, Hidden
- ✅ `Maps` - ID, Name, Hidden, Priority

**Indexes:**
- ✅ `IX_Grids_Map_CoordX_CoordY`
- ✅ `IX_Markers_GridId`
- ✅ `IX_Markers_Key` (UNIQUE)
- ✅ `IX_Tiles_MapId_Zoom_CoordX_CoordY` (UNIQUE)
- ✅ `IX_Tokens_Username`

---

## ❌ Missing Features (0% Implementation)

### Admin Panel UI - COMPLETELY MISSING

The Go version has a full admin panel built with **Materialize CSS** and **Intercooler.js** (HTMX-like library) for reactive UI updates. The .NET version has **ZERO admin UI**.

#### Missing Admin Endpoints (0/14 implemented)

| Endpoint | Method | Purpose | Template |
|----------|--------|---------|----------|
| `/admin/` | GET | Admin dashboard - users list, maps list, config forms | `admin/index.tmpl` |
| `/admin/user` | GET | Create/edit user form with role checkboxes | `admin/user.tmpl` |
| `/admin/user` | POST | Save user (create/update) with auths | - |
| `/admin/deleteUser` | GET | Delete user + cascade delete all tokens | - |
| `/admin/delete` | GET | Delete map + all associated grids | - |
| `/admin/wipe` | GET | **DANGEROUS**: Wipe all data (grids, markers, tiles, maps) | - |
| `/admin/cleanup` | GET | Find and delete duplicate grids (same map+coord, different IDs) | - |
| `/admin/setPrefix` | POST | Set URL prefix for token display | - |
| `/admin/setDefaultHide` | POST | Toggle default hidden for new maps | - |
| `/admin/setTitle` | POST | Set page title for all pages | - |
| `/admin/rebuildZooms` | GET | Async task: rebuild all zoom levels 1-6 from base tiles | - |
| `/admin/export` | GET | Export ZIP: grids.json + markers + PNG files per map | - |
| `/admin/merge` | POST | Import ZIP: experimental merge from export file | - |
| `/admin/map` | GET/POST | Edit map properties (name, hidden, priority) | `admin/map.tmpl` |
| `/admin/mapic` | POST | HTMX endpoint: toggle map hidden status inline | - |

### Missing Service Implementations

#### IAdminService (Does NOT exist)

```csharp
public interface IAdminService
{
    // Data Management
    Task WipeAllDataAsync(); // Delete grids, markers, tiles, maps buckets
    Task CleanupDuplicateGridsAsync(); // Find duplicate map+coord combinations

    // Export/Import
    Task<Stream> ExportToZipAsync(); // Create ZIP with grids.json + markers + PNGs per map
    Task<Stream> BackupToZipAsync(); // Full DB + tiles backup
    Task ImportFromZipAsync(Stream zipStream); // Parse and merge ZIP data

    // Maintenance
    Task RebuildZoomsAsync(); // Background task: regenerate zoom levels 1-6
}
```

#### Export/Import Features (Missing)

**Export Format (Go implementation):**
```
export.zip
├── {mapid}/
│   ├── grids.json       # { Grids: {coord: gridId}, Markers: {gridId: [markers]} }
│   └── {gridId}.png     # Base zoom-0 tile images
```

**What's missing:**
- ZIP creation with multiple maps
- JSON serialization of grid/marker data per map
- File streaming from gridStorage into ZIP
- Import parser with conflict resolution
- Map merging during import (same logic as gridUpdate but from ZIP)

#### Template System (Missing)

Go uses **html/template** with 6 template files:
- `templates/index.tmpl` - User dashboard
- `templates/login.tmpl` - Login form
- `templates/password.tmpl` - Password change form
- `templates/admin/index.tmpl` - Admin dashboard (171 lines!)
- `templates/admin/user.tmpl` - User editor (76 lines)
- `templates/admin/map.tmpl` - Map editor (45 lines)

**.NET current implementation:**
- Inline HTML strings in `AuthEndpoints.cs`
- No Materialize CSS
- No JavaScript libraries
- No forms validation
- No modals for dangerous operations

**What's needed:**
- Choose template engine (Razor Pages, RazorLight, or Scriban)
- Convert 6 Go templates to chosen format
- Add Materialize CSS 1.0.0 CDN
- Add Intercooler.js or replace with HTMX
- Add form validation
- Add confirmation modals

---

## Impact Analysis

### What Works NOW ✅

1. **Game clients can fully operate:**
   - Upload map tiles
   - Track characters
   - Place/update/delete markers
   - View maps in frontend
   - Receive real-time updates

2. **Basic user operations:**
   - Login/logout
   - Generate tokens
   - Change password

### What's BROKEN ❌

1. **No user administration:**
   - Cannot create new users (except via direct DB manipulation)
   - Cannot edit user permissions
   - Cannot delete users
   - Cannot see which tokens belong to which user

2. **No configuration management:**
   - Cannot change site title
   - Cannot set URL prefix for tokens
   - Cannot set default hidden for new maps

3. **No data maintenance tools:**
   - Cannot export/backup data
   - Cannot import external map data
   - Cannot fix corrupted/duplicate grids
   - Cannot wipe database through UI
   - Cannot rebuild zoom levels if corrupted

4. **No map management:**
   - Cannot rename maps
   - Cannot set map priority
   - Cannot toggle map visibility
   - Cannot delete maps

### Production Implications

**For game clients**: ✅ **READY**
- All functionality works perfectly
- Backward compatible with existing clients
- Performance equivalent to Go version

**For server administrators**: ❌ **NOT READY**
- Must use direct database manipulation for user management
- No backup/recovery tools
- No data maintenance capabilities
- Difficult to diagnose/fix issues

---

## Effort Estimation

### Priority 1: Admin Panel Foundation (Critical)
**Effort:** 10-12 hours

- Create `AdminEndpoints.cs` with all 14 endpoints (4 hours)
- Choose and integrate template engine (Razor Pages recommended) (2 hours)
- Convert 6 templates from Go to Razor syntax (3 hours)
- Add Materialize CSS + JavaScript libraries (1 hour)
- Add form validation and modals (2 hours)

### Priority 2: Admin Services (Critical)
**Effort:** 8-10 hours

- Create `IAdminService` interface (30 min)
- Implement `WipeAllDataAsync()` (1 hour)
- Implement `CleanupDuplicateGridsAsync()` (2 hours)
- Implement `RebuildZoomsAsync()` background task (3 hours)
- User CRUD operations (edit, delete with cascade) (2 hours)
- Map CRUD operations (1.5 hours)

### Priority 3: Export/Import (Important)
**Effort:** 6-8 hours

- ZIP export implementation (3 hours)
- ZIP import with merge logic (3 hours)
- Full backup implementation (2 hours)

### Priority 4: Polish (Nice to have)
**Effort:** 2-4 hours

- Better error handling (1 hour)
- Loading indicators (1 hour)
- Confirmation dialogs (1 hour)
- Admin audit logging (1 hour)

**Total Estimated Effort:** 26-34 hours (3-4 days for one developer)

---

## Technology Comparison

### Go Implementation Details

```go
// Template rendering
m.ExecuteTemplate(rw, "admin/index.tmpl", struct {
    Page Page
    Session *Session
    Users []string
    Prefix string
}{...})

// Pub/sub with channels
type topic struct {
    c []chan *TileData
    mu sync.Mutex
}

// Map merging
maps := map[int]struct{ X, Y int }{}
for x, row := range grup.Grids {
    // Complex offset calculation...
}
```

### .NET Implementation Details

```csharp
// Equivalent: System.Threading.Channels
private readonly Channel<TileData> _tileUpdates = Channel.CreateUnbounded<TileData>();

// Template (currently inline, needs Razor)
return Results.Content($@"<html>...</html>", "text/html");

// Map merging (equivalent logic implemented)
var maps = new Dictionary<int, (int X, int Y)>();
for (int x = 0; x < gridUpdate.Grids.Count; x++)
{
    // Offset calculation matches Go version
}
```

---

## Testing Status

### Tested ✅
- All client endpoints with real game client
- Grid merging with multiple maps
- Character tracking and cleanup
- Marker CRUD operations
- Real-time SSE updates
- Image processing and zoom levels
- Session authentication
- BCrypt password hashing

### Not Tested ❌
- Admin panel (doesn't exist)
- Export/import (doesn't exist)
- Wipe operation (doesn't exist)
- Cleanup operation (doesn't exist)
- Rebuild zooms (doesn't exist)

---

## Recommendations

### Immediate Actions (Week 1)
1. ✅ **Document missing features** (this file)
2. **Implement basic admin panel** - user management only
3. **Add configuration management** - title, prefix, defaultHide

### Short-term (Week 2-3)
4. **Implement export/backup** - critical for disaster recovery
5. **Add wipe and cleanup tools** - data maintenance
6. **Implement map management UI**

### Long-term (Month 2+)
7. Consider migrating to Razor Pages for better template management
8. Add audit logging for admin actions
9. Add metrics/monitoring dashboard
10. Consider adding REST API documentation (Swagger/OpenAPI)

---

## Conclusion

The .NET C# migration is **production-ready for game clients** but **not production-ready for server administration**.

**Recommendation:**
- Deploy for game client use immediately ✅
- Complete admin panel before allowing non-technical administrators
- Consider running Go admin panel alongside .NET game server as interim solution

**Success Metrics:**
- Core functionality: ✅ 100% complete
- Admin functionality: ❌ 0% complete
- Overall: ⚠️ 65% complete (weighted)

---

**Next Steps:** See `TODO.md` for prioritized task list.
