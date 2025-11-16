# UI Architecture - Separate Frontend & Backend

## Overview

The HnH Mapper now has a **clean separation** between the UI and API:

- **HnHMapperServer.Api** - Pure backend API (port 8080)
- **HnHMapperServer.Web** - Blazor Server UI (port 5189)

This architecture allows the API to be consumed by **any frontend** (web, mobile, desktop, etc.).

---

## Architecture Diagram

```
┌─────────────────────────────────────────────────┐
│           HnHMapperServer.Web (Port 5189)       │
│              Blazor Server + MudBlazor          │
│                                                 │
│  Components:                                    │
│  ├── Login.razor                                │
│  ├── Index.razor (Dashboard)                    │
│  ├── Password.razor                             │
│  └── Admin/ (Future)                            │
│                                                 │
│  Services:                                      │
│  └── ApiClient (HTTP calls to backend)          │
└─────────────────────────────────────────────────┘
                        ↓ HTTP
                        ↓
┌─────────────────────────────────────────────────┐
│        HnHMapperServer.Api (Port 8080)          │
│           ASP.NET Core Minimal APIs             │
│                                                 │
│  Endpoints:                                     │
│  ├── /login, /logout                            │
│  ├── /generateToken                             │
│  ├── /password                                  │
│  ├── /client/{token}/* (Game client)            │
│  ├── /map/api/* (Map viewer)                    │
│  └── /admin/* (Future)                          │
│                                                 │
│  Services:                                      │
│  ├── AuthenticationService                      │
│  ├── GridService                                │
│  ├── MarkerService                              │
│  └── TileService                                │
└─────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────┐
│              SQLite Database                    │
│         (Users, Sessions, Grids, etc.)          │
└─────────────────────────────────────────────────┘
```

---

## Project Structure

### HnHMapperServer.Api (Backend)

**Purpose:** Pure REST API backend, no UI

**Responsibilities:**
- Handle authentication (sessions, cookies)
- Serve game client endpoints
- Manage map data (grids, markers, tiles)
- Process images and generate zoom levels
- Provide real-time updates via SSE

**Technologies:**
- ASP.NET Core Minimal APIs
- Entity Framework Core + SQLite
- BCrypt.Net for passwords
- ImageSharp for image processing
- System.Threading.Channels for pub/sub

**Endpoints:**
- Authentication: `/login`, `/logout`, `/generateToken`, `/password`
- Game Client: `/client/{token}/*` (9 endpoints)
- Map Viewer: `/map/api/*` (8 endpoints)
- Admin: `/admin/*` (to be implemented)

### HnHMapperServer.Web (Frontend)

**Purpose:** User interface for managing the mapper

**Responsibilities:**
- Display login and dashboard UI
- Call backend API for all operations
- Render forms and tables
- Show real-time notifications

**Technologies:**
- Blazor Server (interactive server-side rendering)
- MudBlazor (Material Design component library)
- HttpClient for API communication

**Pages:**
- `/login` - Login form
- `/` - Dashboard with tokens list
- `/password` - Password change form
- `/admin` - Admin panel (to be implemented)

---

## How They Work Together

### 1. Authentication Flow

```
User enters credentials in /login
    ↓
Web UI calls ApiClient.LoginAsync()
    ↓
ApiClient sends POST to API /login
    ↓
API validates credentials, creates session
    ↓
API returns session cookie
    ↓
HttpClient stores cookie for future requests
    ↓
User is logged in, redirected to dashboard
```

### 2. Token Generation Flow

```
User clicks "Generate Token" button
    ↓
Dashboard calls ApiClient.GenerateTokenAsync()
    ↓
ApiClient sends POST to API /generateToken
    ↓
API creates new token in database
    ↓
API returns token string
    ↓
Dashboard shows success message
```

### 3. API Communication

All communication between Web and Api uses **HttpClient** with:

- **Base URL:** Configured in `appsettings.json` (`ApiBaseUrl: "http://localhost:8080"`)
- **Cookies:** Automatically managed by HttpClient for session persistence
- **Content Types:** `application/x-www-form-urlencoded` for forms, `application/json` for data

---

## Configuration

### Backend (HnHMapperServer.Api)

**File:** `appsettings.json`

```json
{
  "GridStorage": "map",
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

**Environment Variables:**
- `HNHMAP_PORT` - Port to listen on (default: 8080)
- `GridStorage` - Directory for map data (default: "map")

### Frontend (HnHMapperServer.Web)

**File:** `appsettings.json`

```json
{
  "ApiBaseUrl": "http://localhost:8080",
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

**Configuration:**
- `ApiBaseUrl` - URL of the backend API

---

## Running the Projects

### Development (Local)

**1. Start the Backend API:**

```bash
cd HnHMapperServer/src/HnHMapperServer.Api
dotnet run
```

Server starts on `http://localhost:8080`

**2. Start the Frontend UI:**

```bash
cd HnHMapperServer/src/HnHMapperServer.Web
dotnet run
```

UI starts on `http://localhost:5189`

**3. Open Browser:**

Navigate to `http://localhost:5189` to access the UI.

### Production Deployment

**Option 1: Same Server**

```bash
# Run API on port 8080
dotnet run --project src/HnHMapperServer.Api

# Run Web UI on port 80
dotnet run --project src/HnHMapperServer.Web --urls "http://0.0.0.0:80"
```

**Option 2: Separate Servers**

```bash
# Server 1 - API Backend
dotnet run --project src/HnHMapperServer.Api --urls "http://0.0.0.0:8080"

# Server 2 - Web UI
# Update appsettings.json: "ApiBaseUrl": "http://api-server:8080"
dotnet run --project src/HnHMapperServer.Web --urls "http://0.0.0.0:80"
```

**Option 3: Docker Compose**

```yaml
version: '3.8'
services:
  api:
    build:
      context: ./src/HnHMapperServer.Api
    ports:
      - "8080:8080"
    volumes:
      - ./map:/map

  web:
    build:
      context: ./src/HnHMapperServer.Web
    ports:
      - "80:8080"
    environment:
      - ApiBaseUrl=http://api:8080
    depends_on:
      - api
```

---

## Benefits of This Architecture

### 1. Reusable API

The backend API can be consumed by:
- **Blazor UI** (current)
- **React/Vue/Angular** frontends
- **Mobile apps** (iOS, Android)
- **Desktop apps** (WPF, Avalonia)
- **Command-line tools**
- **Third-party integrations**

### 2. Technology Flexibility

You can replace the Blazor UI with any frontend technology without touching the backend.

### 3. Independent Scaling

- Scale the API independently for heavy game client traffic
- Scale the Web UI independently for many admins

### 4. Clear Separation of Concerns

- **Backend:** Data, business logic, authentication
- **Frontend:** UI, user experience, presentation

### 5. Easier Testing

- Test API endpoints independently
- Mock API responses in UI tests
- Separate integration tests

---

## API Client Service

The `ApiClient` service in `HnHMapperServer.Web/Services/ApiClient.cs` handles all API communication:

```csharp
public interface IApiClient
{
    Task<bool> LoginAsync(string username, string password);
    Task LogoutAsync();
    Task<List<UserTokenDto>> GetTokensAsync();
    Task<string?> GenerateTokenAsync();
    Task<bool> ChangePasswordAsync(string newPassword);
    bool IsAuthenticated { get; }
    string? CurrentUsername { get; }
}
```

**Usage in Components:**

```razor
@inject IApiClient ApiClient

@code {
    private async Task Login()
    {
        var success = await ApiClient.LoginAsync(username, password);
        if (success)
        {
            // Redirect to dashboard
        }
    }
}
```

---

## MudBlazor Components Used

### Forms
- `MudTextField` - Input fields with validation
- `MudButton` - Buttons with loading states
- `MudPaper` - Card-like containers
- `EditForm` - Form validation

### Feedback
- `MudAlert` - Error/success messages
- `MudSnackbar` - Toast notifications
- `MudProgressCircular` - Loading spinners

### Data Display
- `MudTable` - Tables with sorting/filtering
- `MudGrid` / `MudItem` - Responsive layout

### Navigation
- `MudIcon` - Material Design icons
- `MudText` - Typography with variants

---

## Next Steps

### Immediate

- [ ] Test login flow end-to-end
- [ ] Implement token list parsing (currently returns empty)
- [ ] Add proper authentication state management
- [ ] Create AuthenticationStateProvider

### Admin Panel

- [ ] Create admin dashboard UI
- [ ] User management pages (CRUD)
- [ ] Map management pages
- [ ] Configuration management
- [ ] Export/import functionality

### Enhancements

- [ ] Add loading states everywhere
- [ ] Add proper error handling and logging
- [ ] Create reusable components
- [ ] Add dark mode toggle
- [ ] Add real-time notifications via SignalR

---

## Troubleshooting

### API not reachable from Web UI

**Problem:** `HttpClient` cannot connect to API

**Solution:**
1. Check `appsettings.json` has correct `ApiBaseUrl`
2. Ensure API is running on specified port
3. Check firewall settings
4. Verify CORS if API and Web on different domains

### Session not persisting

**Problem:** Login works but gets logged out immediately

**Solution:**
1. Ensure `HttpClient` is configured with `HttpClientHandler` that supports cookies
2. Check API returns proper `Set-Cookie` headers
3. Verify Web UI sends cookies with subsequent requests

### MudBlazor styles not loading

**Problem:** UI looks unstyled

**Solution:**
1. Check `App.razor` includes MudBlazor CSS: `<link href="_content/MudBlazor/MudBlazor.min.css" />`
2. Check `App.razor` includes MudBlazor JS: `<script src="_content/MudBlazor/MudBlazor.min.js"></script>`
3. Check `Routes.razor` includes providers: `<MudThemeProvider />`, `<MudDialogProvider />`, `<MudSnackbarProvider />`

---

## Summary

The HnH Mapper now has a **modern, maintainable architecture**:

✅ **Backend API** - Reusable REST endpoints
✅ **Frontend UI** - Beautiful Blazor + MudBlazor interface
✅ **Clean Separation** - API can be consumed by any client
✅ **Scalable** - Independent deployment and scaling
✅ **Testable** - Easy to test each layer

This foundation makes it easy to add new features, swap technologies, and maintain the codebase long-term.

---

**Last Updated:** 2025-10-25
