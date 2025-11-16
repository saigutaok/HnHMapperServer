# Admin Endpoints Specification

Complete specification for implementing the admin panel endpoints in the .NET C# version.

---

## Endpoint Overview

| Endpoint | Method | Authorization | Purpose |
|----------|--------|--------------|---------|
| `/admin/` | GET | AUTH_ADMIN | Admin dashboard |
| `/admin/user` | GET | AUTH_ADMIN | User editor form |
| `/admin/user` | POST | AUTH_ADMIN | Save user |
| `/admin/deleteUser` | GET | AUTH_ADMIN | Delete user |
| `/admin/map` | GET | AUTH_ADMIN | Map editor form |
| `/admin/map` | POST | AUTH_ADMIN | Save map |
| `/admin/delete` | GET | AUTH_ADMIN | Delete map |
| `/admin/wipe` | GET | AUTH_ADMIN | Wipe all data |
| `/admin/cleanup` | GET | AUTH_ADMIN | Cleanup duplicates |
| `/admin/setPrefix` | POST | AUTH_ADMIN | Set URL prefix |
| `/admin/setDefaultHide` | POST | AUTH_ADMIN | Set default hide |
| `/admin/setTitle` | POST | AUTH_ADMIN | Set page title |
| `/admin/rebuildZooms` | GET | AUTH_ADMIN | Rebuild zoom levels |
| `/admin/export` | GET | AUTH_ADMIN | Export data |
| `/admin/merge` | POST | AUTH_ADMIN | Import/merge data |
| `/admin/mapic` | POST | AUTH_ADMIN | Toggle map hidden (HTMX) |

---

## 1. Admin Dashboard

### `GET /admin/`

**Authorization:** AUTH_ADMIN required

**Response:** HTML page (Razor view)

**Template Data:**
```csharp
public class AdminIndexViewModel
{
    public Page Page { get; set; }              // Title
    public Session Session { get; set; }         // Current user
    public List<string> Users { get; set; }      // All usernames
    public string Prefix { get; set; }           // URL prefix for tokens
    public bool DefaultHide { get; set; }        // Default hide new maps
    public List<MapInfo> Maps { get; set; }      // All maps
}
```

**Template Features:**
- Users table with Edit button for each user
- "Add user" button → `/admin/user`
- Maps table with columns: Name, Actions (Toggle Hidden, Edit, Delete)
- Configuration forms:
  - Set prefix (text input + Save button)
  - Set default hide (checkbox + Save button)
  - Set title (text input + Save button)
- Dangerous operations section:
  - Wipe all data (with confirmation modal)
  - Cleanup multi-ID grids (with confirmation)
  - Rebuild zooms (with warning)
  - Export data (download button)
  - Merge/import (file upload form)

**Go Template Reference:** `templates/admin/index.tmpl` (171 lines)

---

## 2. User Editor Form

### `GET /admin/user?user={username}`

**Authorization:** AUTH_ADMIN required

**Query Parameters:**
- `user` (optional) - Username to edit. If omitted, show form to create new user.

**Response:** HTML page (Razor view)

**Template Data:**
```csharp
public class UserEditorViewModel
{
    public Page Page { get; set; }
    public Session Session { get; set; }
    public User User { get; set; }           // User data (or empty for new user)
    public string Username { get; set; }     // Username (empty for new user)
}
```

**Form Fields:**
- Username (text input, **disabled** if editing existing user)
- Password (password input, placeholder: "Leave blank to keep current")
- Roles (checkboxes):
  - [ ] Map - Can view map
  - [ ] Markers - Can see markers
  - [ ] Characters - Can see character positions
  - [ ] Upload - Can generate tokens and upload data
  - [ ] Redactor - Can edit/delete tiles and markers
  - [ ] Admin - Full admin access
- Save button
- Delete button (only if editing existing user, not for new user)

**Form Action:** `POST /admin/user`

**Delete Action:** `GET /admin/deleteUser?user={username}`

**Go Template Reference:** `templates/admin/user.tmpl` (76 lines)

---

## 3. Save User

### `POST /admin/user`

**Authorization:** AUTH_ADMIN required

**Request:** Form data
```
user=testuser
pass=newpassword123          (optional, empty = keep current password)
auths=map                    (can have multiple, checkbox values)
auths=upload
auths=admin
```

**Logic:**
1. Get user from database (or create new if doesn't exist)
2. If password provided and not empty:
   - Hash with BCrypt
   - Update User.PasswordHash
3. Update User.Auths array from form checkboxes
4. Save user to database
5. **SPECIAL CASE**: If current session user is "admin" with TempAdmin=true:
   - This is first user creation
   - Delete TempAdmin session
   - User must re-login with new account
6. If editing current user's own account:
   - Update session.Auths to match new auths
7. Redirect to `/admin/`

**Response:** 302 Redirect to `/admin/`

**C# Implementation:**
```csharp
private static async Task<IResult> SaveUser(
    HttpContext context,
    [FromForm] string user,
    [FromForm] string pass,
    [FromForm] List<string> auths,
    IUserRepository userRepository,
    ISessionRepository sessionRepository,
    IAuthenticationService authService)
{
    var session = context.GetSession();
    if (session == null || !session.Auths.Has(AuthRole.Admin))
        return Results.Redirect("/");

    var existingUser = await userRepository.GetUserAsync(user);
    var isNew = existingUser == null;

    var userObj = existingUser ?? new User { Username = user };

    // Update password if provided
    if (!string.IsNullOrEmpty(pass))
    {
        userObj.PasswordHash = authService.HashPassword(pass);
    }

    // Update auths
    userObj.Auths = new Auths(auths);

    await userRepository.SaveUserAsync(userObj);

    // Handle TempAdmin removal
    if (session.TempAdmin && isNew)
    {
        await sessionRepository.DeleteSessionAsync(session.Id);
    }

    // Update current session if editing self
    if (session.Username == user)
    {
        session.Auths = userObj.Auths;
        await sessionRepository.SaveSessionAsync(session);
    }

    return Results.Redirect("/admin/");
}
```

---

## 4. Delete User

### `GET /admin/deleteUser?user={username}`

**Authorization:** AUTH_ADMIN required

**Query Parameters:**
- `user` (required) - Username to delete

**Logic:**
1. Load user from database
2. Delete all tokens associated with user (from Tokens table)
3. Delete user record
4. If deleting current user:
   - Delete current session
   - User is logged out
5. Redirect to `/admin/`

**Response:** 302 Redirect to `/admin/`

**C# Implementation:**
```csharp
private static async Task<IResult> DeleteUser(
    HttpContext context,
    [FromQuery] string user,
    IUserRepository userRepository,
    ISessionRepository sessionRepository)
{
    var session = context.GetSession();
    if (session == null || !session.Auths.Has(AuthRole.Admin))
        return Results.Redirect("/");

    // Delete user will cascade delete tokens
    await userRepository.DeleteUserAsync(user);

    // If deleting self, delete session
    if (session.Username == user)
    {
        await sessionRepository.DeleteSessionAsync(session.Id);
    }

    return Results.Redirect("/admin/");
}
```

---

## 5. Map Editor Form

### `GET /admin/map?map={mapId}`

**Authorization:** AUTH_ADMIN required

**Query Parameters:**
- `map` (required) - Map ID to edit

**Response:** HTML page (Razor view)

**Template Data:**
```csharp
public class MapEditorViewModel
{
    public Page Page { get; set; }
    public Session Session { get; set; }
    public MapInfo MapInfo { get; set; }
}
```

**Form Fields:**
- Map ID (hidden input)
- Name (text input)
- Hidden (checkbox) - If checked, map won't appear in frontend map list
- Priority (checkbox) - If checked, this map takes precedence during map merging

**Form Action:** `POST /admin/map`

**Go Template Reference:** `templates/admin/map.tmpl` (45 lines)

---

## 6. Save Map

### `POST /admin/map`

**Authorization:** AUTH_ADMIN required

**Request:** Form data
```
map=1
name=Main World
hidden=true              (optional, checkbox)
priority=true            (optional, checkbox)
```

**Logic:**
1. Parse map ID from form
2. Load MapInfo from database
3. Update Name, Hidden, Priority from form
   - Hidden: `!(formValue == "")` (checkbox present = true)
   - Priority: `!(formValue == "")` (checkbox present = true)
4. Save MapInfo to database
5. Redirect to `/admin/`

**Response:** 302 Redirect to `/admin/`

---

## 7. Delete Map

### `GET /admin/delete?mapid={mapId}`

**Authorization:** AUTH_ADMIN required

**Query Parameters:**
- `mapid` (required) - Map ID to delete

**Logic:**
1. Delete all grids with Grid.Map == mapId
2. Delete map record (MapInfo with ID == mapId)
3. Note: Tiles are orphaned but not deleted (cleanup in future rebuild)
4. Redirect to `/admin/`

**Response:** 302 Redirect to `/admin/`

---

## 8. Wipe All Data

### `GET /admin/wipe`

**Authorization:** AUTH_ADMIN required

**WARNING:** This is a **DESTRUCTIVE** operation!

**Logic:**
1. Delete all records from:
   - Grids table
   - Markers table
   - Tiles table
   - Maps table
2. Do NOT delete:
   - Users
   - Sessions
   - Tokens
   - Config
3. Redirect to `/admin/`

**Response:** 302 Redirect to `/admin/`

**Confirmation:** Should have modal confirmation in UI (see template)

**C# Implementation:**
```csharp
private static async Task<IResult> WipeAllData(
    HttpContext context,
    IAdminService adminService)
{
    var session = context.GetSession();
    if (session == null || !session.Auths.Has(AuthRole.Admin))
        return Results.Redirect("/");

    await adminService.WipeAllDataAsync();

    return Results.Redirect("/admin/");
}
```

---

## 9. Cleanup Duplicate Grids

### `GET /admin/cleanup`

**Authorization:** AUTH_ADMIN required

**Purpose:** Find and delete grids where multiple GridIDs point to same (MapId, CoordX, CoordY)

**Logic:**
1. Query all grids, group by (MapId, CoordX, CoordY)
2. For each group with count > 1:
   - Keep first GridID
   - Delete all other GridIDs
   - Delete tiles for deleted grids
   - Mark coordinate for zoom rebuild
3. Rebuild affected zoom levels
4. Redirect to `/admin/`

**Response:** 302 Redirect to `/admin/`

**Logging:** Log count of deleted duplicates

---

## 10. Set URL Prefix

### `POST /admin/setPrefix`

**Authorization:** AUTH_ADMIN required

**Request:** Form data
```
prefix=https://myserver.com
```

**Logic:**
1. Save "prefix" to Config table
2. This prefix is used in user dashboard to show full token URLs
3. Redirect to `/admin/`

**Response:** 302 Redirect to `/admin/`

---

## 11. Set Default Hide

### `POST /admin/setDefaultHide`

**Authorization:** AUTH_ADMIN required

**Request:** Form data
```
defaultHide=true        (optional, checkbox)
```

**Logic:**
1. If checkbox present:
   - Save "defaultHide" = "true" to Config table
2. Else:
   - Delete "defaultHide" key from Config table
3. This setting determines if new maps are created with Hidden=true
4. Redirect to `/admin/`

**Response:** 302 Redirect to `/admin/`

---

## 12. Set Title

### `POST /admin/setTitle`

**Authorization:** AUTH_ADMIN required

**Request:** Form data
```
title=My HnH Server
```

**Logic:**
1. Save "title" to Config table
2. This title appears in page `<title>` tags
3. Redirect to `/admin/`

**Response:** 302 Redirect to `/admin/`

---

## 13. Rebuild Zoom Levels

### `GET /admin/rebuildZooms`

**Authorization:** AUTH_ADMIN required

**Purpose:** Regenerate all zoom levels 1-6 from base tiles (zoom 0)

**Logic:**
1. Start background task (don't wait for completion)
2. Delete all tiles where Zoom > 0
3. For each grid at zoom 0:
   - Add to processing queue for zoom 1
4. For zoom levels 1-6:
   - For each coordinate to process:
     - Load 4 sub-tiles (2x2 grid)
     - Combine using BiLinear scaling to 100x100
     - Save combined tile
     - Add parent coordinate to next zoom level queue
5. Log progress every 100 tiles
6. Redirect immediately (don't wait for completion)

**Response:** 302 Redirect to `/admin/` (before background task completes)

**Logging:**
```
Rebuild Zooms...
Rebuild Zooms Saving...
Rebuild Zooms: 1
Rebuild Zooms: 2
...
Rebuild Zooms Finish!
```

---

## 14. Export Data

### `GET /admin/export`

**Authorization:** AUTH_ADMIN required

**Response:** ZIP file download

**Content-Type:** `application/zip`

**Content-Disposition:** `attachment; filename="griddata.zip"`

**ZIP Structure:**
```
griddata.zip
├── 0/
│   ├── grids.json
│   ├── abc123.png
│   └── def456.png
├── 1/
│   ├── grids.json
│   └── ghi789.png
...
```

**grids.json Format:**
```json
{
  "Grids": {
    "10_20": "abc123",
    "10_21": "def456"
  },
  "Markers": {
    "abc123": [
      {
        "Name": "Silver Vein",
        "GridID": "abc123",
        "Position": {"x": 50, "y": 30},
        "Image": "gfx/terobjs/mm/argentite",
        "Ready": false,
        "MaxReady": -1,
        "MinReady": -1
      }
    ]
  }
}
```

**Logic:**
1. Create in-memory ZIP stream
2. Query all maps
3. For each map:
   - Query all grids for map
   - Query all markers for those grids
   - Build Grids dictionary: coord_name → gridId
   - Build Markers dictionary: gridId → [marker array]
   - Serialize to JSON, add `{mapId}/grids.json` to ZIP
4. For each grid with tile at zoom 0:
   - Read PNG from `gridStorage/grids/{gridId}.png`
   - Add `{mapId}/{gridId}.png` to ZIP
5. Return ZIP stream

---

## 15. Import/Merge Data

### `POST /admin/merge`

**Authorization:** AUTH_ADMIN required

**Request:** Multipart form data
```
merge=<file upload>
```

**Content-Type:** `multipart/form-data`

**Logic:**
1. Read uploaded ZIP file
2. For each `{mapId}/grids.json` entry:
   - Deserialize JSON
   - Run map detection (same logic as `/client/{token}/gridUpdate`):
     - Check which existing maps overlap with imported grids
     - If no overlap: create new map
     - If overlap: merge into existing map with offset calculation
   - Import grids with adjusted coordinates
   - Import markers
3. For each `{mapId}/*.png` entry:
   - Extract to `gridStorage/grids/`
   - Create tile record at zoom 0
4. Trigger rebuild zooms
5. Redirect to `/admin/`

**Response:** 302 Redirect to `/admin/`

**Warning:** This is experimental and can cause data conflicts!

---

## 16. Toggle Map Hidden (HTMX)

### `POST /admin/mapic?map={mapId}&action=toggle-hidden`

**Authorization:** AUTH_ADMIN required

**Query Parameters:**
- `map` (required) - Map ID
- `action` (required) - Must be "toggle-hidden"

**Purpose:** HTMX/Intercooler.js endpoint for reactive UI update

**Logic:**
1. Load MapInfo by ID
2. Toggle Hidden: `mapInfo.Hidden = !mapInfo.Hidden`
3. Save MapInfo
4. Return HTML fragment (NOT full page):

**Response:** HTML fragment for button replacement
```html
{{if .Hidden}}Show{{else}}Hide{{end}}
```

**Go Template Reference:** Uses inline template block `admin/index.tmpl:toggle-hidden`

**HTMX/Intercooler.js:** Button with `ic-post-to="/admin/mapic?map=1&action=toggle-hidden"` replaces itself with response

---

## Authorization Middleware

All admin endpoints should be protected with authorization check:

```csharp
public static void MapAdminEndpoints(this IEndpointRouteBuilder app)
{
    var group = app.MapGroup("/admin");

    // Add authorization policy
    group.AddEndpointFilter(async (context, next) =>
    {
        var httpContext = context.HttpContext;
        var session = httpContext.GetSession();

        if (session == null || !session.Auths.Has(AuthRole.Admin))
        {
            return Results.Redirect("/");
        }

        return await next(context);
    });

    // Map endpoints...
}
```

---

## Template System Integration

### Recommended: Razor Pages

**Directory Structure:**
```
src/HnHMapperServer.Api/
├── Pages/
│   ├── Admin/
│   │   ├── Index.cshtml          (Dashboard)
│   │   ├── Index.cshtml.cs       (PageModel)
│   │   ├── User.cshtml           (User editor)
│   │   ├── User.cshtml.cs        (PageModel)
│   │   ├── Map.cshtml            (Map editor)
│   │   └── Map.cshtml.cs         (PageModel)
│   ├── Index.cshtml              (User dashboard)
│   ├── Login.cshtml              (Login form)
│   └── Password.cshtml           (Password change)
└── wwwroot/
    ├── css/
    │   └── materialize.min.css   (or use CDN)
    └── js/
        ├── zepto.min.js
        └── intercooler-1.2.1.min.js
```

**Program.cs additions:**
```csharp
builder.Services.AddRazorPages();

app.MapRazorPages();
```

---

## Testing

### Manual Test Scenarios

1. **User Management:**
   - Create new user with various role combinations
   - Edit existing user, change roles
   - Change user password
   - Delete user, verify tokens deleted
   - Verify TempAdmin removal on first user creation

2. **Map Management:**
   - Edit map name
   - Toggle map hidden
   - Toggle map priority
   - Delete map, verify grids deleted

3. **Configuration:**
   - Change prefix, verify shows in user dashboard
   - Change title, verify shows in browser tab
   - Toggle default hide, verify new maps respect setting

4. **Maintenance:**
   - Export data, verify ZIP contents
   - Import data, verify merge logic works
   - Wipe data, verify tables cleared but users remain
   - Cleanup duplicates (need to create duplicates first)
   - Rebuild zooms, verify images generated

---

**Last Updated:** 2025-10-25
