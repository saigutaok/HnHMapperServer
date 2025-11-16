# TODO: Complete HnH Mapper .NET Migration

**Current Status**: Core functionality 100% complete, Admin Panel 0% complete
**Priority**: Admin panel is critical for production server administration

---

## Priority 1: Blazor UI Foundation [CRITICAL]

### 1.1 Setup Blazor + MudBlazor (2 hours)
- [ ] Add MudBlazor NuGet package (v6.11.2)
- [ ] Update `Program.cs`:
  - [ ] Add `AddRazorComponents()` + `AddInteractiveServerComponents()`
  - [ ] Add `AddMudServices()`
  - [ ] Add `MapRazorComponents<App>()`
- [ ] Create component directory structure:
  - [ ] `Components/App.razor` (root component)
  - [ ] `Components/Routes.razor` (routing)
  - [ ] `Components/_Imports.razor` (global usings)
  - [ ] `Components/Layout/MainLayout.razor`
  - [ ] `Components/Layout/NavMenu.razor`

### 1.2 Authentication Integration (1-2 hours)
- [ ] Create `ServerAuthenticationStateProvider.cs`
- [ ] Register `AuthenticationStateProvider` in DI
- [ ] Add `AddAuthorizationCore()` to services
- [ ] Add `AddHttpContextAccessor()` for session access
- [ ] Test authentication with `[Authorize]` attribute

### 1.3 Convert Existing Pages to Blazor (3-4 hours)
- [ ] `Components/Pages/Login.razor` (replace `AuthEndpoints.cs` HTML)
  - MudPaper card with centered form
  - MudTextField for username/password
  - Error alert display
  - Remember to disable antiforgery on old endpoint during transition
- [ ] `Components/Pages/Index.razor` (user dashboard)
  - Display token list in MudTable
  - "Generate Token" button
  - Links to password change, admin panel, map viewer
- [ ] `Components/Pages/Password.razor` (password change)
  - MudTextField for new password
  - Confirmation dialog
  - Success snackbar notification

### 1.3 Create AdminEndpoints.cs (4-5 hours)
```csharp
// File: src/HnHMapperServer.Api/Endpoints/AdminEndpoints.cs
public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/admin").RequireAuthorization("Admin");

        // Dashboard
        group.MapGet("/", AdminIndex);

        // User Management
        group.MapGet("/user", UserForm);
        group.MapPost("/user", SaveUser);
        group.MapGet("/deleteUser", DeleteUser);

        // Map Management
        group.MapGet("/map", MapForm);
        group.MapPost("/map", SaveMap);
        group.MapGet("/delete", DeleteMap);

        // Configuration
        group.MapPost("/setPrefix", SetPrefix).DisableAntiforgery();
        group.MapPost("/setDefaultHide", SetDefaultHide).DisableAntiforgery();
        group.MapPost("/setTitle", SetTitle).DisableAntiforgery();

        // Maintenance Operations
        group.MapGet("/wipe", WipeAllData);
        group.MapGet("/cleanup", CleanupDuplicates);
        group.MapGet("/rebuildZooms", RebuildZooms);

        // Export/Import
        group.MapGet("/export", ExportData);
        group.MapPost("/merge", ImportMerge).DisableAntiforgery();

        // HTMX Endpoints
        group.MapPost("/mapic", MapInteractive).DisableAntiforgery();
    }

    // TODO: Implement all endpoint handlers
}
```

**Endpoints to implement:**
- [ ] `AdminIndex()` - Dashboard with lists and forms
- [ ] `UserForm(string? user)` - GET user editor
- [ ] `SaveUser(UserFormDto)` - POST save user
- [ ] `DeleteUser(string user)` - DELETE user + tokens
- [ ] `MapForm(int map)` - GET map editor
- [ ] `SaveMap(MapFormDto)` - POST save map
- [ ] `DeleteMap(int mapid)` - DELETE map + grids
- [ ] `SetPrefix(string prefix)` - Update config
- [ ] `SetDefaultHide(bool defaultHide)` - Update config
- [ ] `SetTitle(string title)` - Update config
- [ ] `WipeAllData()` - DANGEROUS: Delete all
- [ ] `CleanupDuplicates()` - Fix duplicate grids
- [ ] `RebuildZooms()` - Regenerate zoom levels
- [ ] `ExportData()` - Create ZIP export
- [ ] `ImportMerge(IFormFile)` - Import from ZIP
- [ ] `MapInteractive()` - Toggle hidden via HTMX

---

## Priority 2: Admin Service Layer [CRITICAL]

### 2.1 Create IAdminService Interface (30 min)
```csharp
// File: src/HnHMapperServer.Services/Interfaces/IAdminService.cs
public interface IAdminService
{
    // Data Maintenance
    Task WipeAllDataAsync();
    Task CleanupDuplicateGridsAsync();
    Task RebuildZoomsAsync();

    // User Management
    Task<User?> GetUserWithTokensAsync(string username);
    Task SaveUserAsync(string username, string? password, List<string> auths);
    Task DeleteUserAsync(string username); // Cascade delete tokens
    Task<List<string>> GetAllUsernamesAsync();

    // Map Management
    Task DeleteMapAsync(int mapId); // Cascade delete grids

    // Export/Import
    Task<Stream> ExportToZipAsync();
    Task<Stream> BackupToZipAsync();
    Task ImportFromZipAsync(Stream zipStream);
}
```

### 2.2 Implement AdminService (8-10 hours)
- [ ] Create `src/HnHMapperServer.Services/Services/AdminService.cs`
- [ ] Implement **WipeAllDataAsync()** (1 hour)
  - Delete all records from: Grids, Markers, Tiles, Maps tables
  - Keep: Users, Sessions, Config, Tokens
  - Return confirmation message
- [ ] Implement **CleanupDuplicateGridsAsync()** (2 hours)
  - Query: Find grids with duplicate (MapId, CoordX, CoordY)
  - Keep only first occurrence of each duplicate
  - Delete duplicate grid IDs
  - Delete associated tiles
  - Rebuild zoom levels for affected coordinates
  - Return count of deleted duplicates
- [ ] Implement **RebuildZoomsAsync()** (3 hours)
  - Background task (return immediately, run in background)
  - Delete all tiles where Zoom > 0
  - For each grid at zoom 0:
    - Calculate parent coordinates
    - Mark for zoom rebuild
  - For zoom levels 1-6:
    - For each marked coordinate:
      - Load 4 sub-tiles
      - Combine using ImageSharp BiLinear scaling
      - Save parent tile
      - Mark parent's parent for next zoom level
  - Log progress every 100 tiles
- [ ] Implement **User Management Methods** (2 hours)
  - `GetUserWithTokensAsync()` - Load user + tokens
  - `SaveUserAsync()` - Create or update user
    - Hash password with BCrypt if provided
    - Update auths array
    - Handle TempAdmin removal logic (like Go version)
  - `DeleteUserAsync()` - Delete user + all tokens
  - `GetAllUsernamesAsync()` - For admin dashboard list
- [ ] Implement **Map Management** (1.5 hours)
  - `DeleteMapAsync()` - Delete map + cascade delete all grids with that mapId

### 2.3 Implement Export/Import (6-8 hours)
- [ ] **ExportToZipAsync()** (3 hours)
  - Create ZIP archive in memory
  - For each map:
    - Query all grids for that map
    - Query all markers for those grids
    - Create JSON: `{ Grids: {coord: gridId}, Markers: {gridId: [markers]} }`
    - Add `{mapId}/grids.json` to ZIP
    - For each grid with tile:
      - Read PNG from `gridStorage/grids/{gridId}.png`
      - Add `{mapId}/{gridId}.png` to ZIP
  - Return ZIP stream
- [ ] **BackupToZipAsync()** (2 hours)
  - Create ZIP archive
  - Add entire SQLite database file: `grids.db`
  - Add all tiles from `gridStorage/` recursively
  - Return ZIP stream
- [ ] **ImportFromZipAsync()** (3 hours)
  - Parse ZIP entries
  - For each `{mapId}/grids.json`:
    - Deserialize grid/marker data
    - Run map detection logic (same as gridUpdate)
    - Determine target map (existing or create new)
    - Calculate offsets if merging
    - Import grids with adjusted coordinates
    - Import markers
  - For each `{mapId}/*.png`:
    - Copy to `gridStorage/grids/`
    - Create tile records
  - Trigger rebuild zooms after import

---

## Priority 3: Enhanced Features [IMPORTANT]

### 3.1 User Management Improvements (2 hours)
- [ ] Add user list with search/filter
- [ ] Show last login time (need to track in Sessions table)
- [ ] Show token count per user
- [ ] Add "Reset Password" functionality
- [ ] Add user activity log

### 3.2 Map Management Improvements (2 hours)
- [ ] Show map statistics (grid count, tile count, marker count)
- [ ] Show map size in coordinates (min/max X/Y)
- [ ] Add map thumbnail generation
- [ ] Add bulk map operations (hide/show multiple)

### 3.3 Configuration Improvements (1 hour)
- [ ] Move all config to database (remove appsettings.json dependency where possible)
- [ ] Add "Test Connection" for external services
- [ ] Add configuration export/import
- [ ] Add configuration validation

### 3.4 Better Error Handling (2 hours)
- [ ] Add global exception handler
- [ ] Return user-friendly error messages
- [ ] Add admin error log viewer
- [ ] Add retry logic for transient failures

---

## Priority 4: Polish & Quality of Life [NICE TO HAVE]

### 4.1 UI Improvements (3 hours)
- [ ] Add loading spinners for long operations
- [ ] Add progress bars for export/import
- [ ] Add toast notifications for success/error
- [ ] Add confirmation modals for dangerous operations
- [ ] Add keyboard shortcuts (Ctrl+S to save, etc.)
- [ ] Add dark mode toggle

### 4.2 Audit & Logging (2 hours)
- [ ] Add admin action audit log
  - Track: User, Action, Timestamp, IP Address
  - Actions: User create/edit/delete, Map delete, Wipe, etc.
- [ ] Add audit log viewer in admin panel
- [ ] Add audit log export

### 4.3 Monitoring & Metrics (3 hours)
- [ ] Add dashboard with metrics:
  - Total users
  - Total maps
  - Total grids
  - Total markers
  - Disk usage
  - Active sessions
- [ ] Add performance metrics
- [ ] Add health check endpoint (`/health`)

### 4.4 Documentation (2 hours)
- [ ] Add inline help text in admin forms
- [ ] Create admin user guide
- [ ] Add API documentation (Swagger/OpenAPI)
- [ ] Create video tutorials for common tasks

---

## Testing Checklist

### Manual Testing
- [ ] Test user creation with all role combinations
- [ ] Test user editing and password change
- [ ] Test user deletion (verify tokens also deleted)
- [ ] Test map creation and editing
- [ ] Test map deletion (verify grids also deleted)
- [ ] Test wipe operation
- [ ] Test cleanup duplicates
- [ ] Test rebuild zooms
- [ ] Test export (verify ZIP contents)
- [ ] Test import (verify data merged correctly)
- [ ] Test all configuration changes
- [ ] Test HTMX toggle hidden functionality

### Automated Testing (Future)
- [ ] Add unit tests for AdminService
- [ ] Add integration tests for admin endpoints
- [ ] Add E2E tests with Playwright/Selenium
- [ ] Add load tests for export/import

---

## Deployment Checklist

### Pre-Deployment
- [ ] Complete all Priority 1 tasks
- [ ] Complete all Priority 2 tasks
- [ ] Test export/backup functionality
- [ ] Create initial backup of production data
- [ ] Document rollback procedure

### Deployment
- [ ] Deploy new version
- [ ] Create first admin user
- [ ] Verify admin panel loads
- [ ] Test critical operations (user management, export)
- [ ] Monitor logs for errors

### Post-Deployment
- [ ] Create full backup
- [ ] Test restore from backup
- [ ] Train administrators on new UI
- [ ] Document any issues encountered

---

## Progress Tracking

**Completion Status:**
- [ ] Priority 1: 0% (0/3 sections complete)
- [ ] Priority 2: 0% (0/3 sections complete)
- [ ] Priority 3: 0% (0/4 sections complete)
- [ ] Priority 4: 0% (0/4 sections complete)

**Total Estimated Hours:** 26-34 hours

**Target Completion Date:** _[To be determined]_

---

## Notes

- Keep backward compatibility with existing game clients
- Don't modify core client endpoints during admin panel work
- Test export/import with real data before deploying
- Consider running Go admin panel alongside .NET server during transition
- Prioritize data safety over feature completeness

---

**Last Updated:** 2025-10-25
