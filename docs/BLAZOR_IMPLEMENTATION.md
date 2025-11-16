# Blazor + MudBlazor Implementation Guide

Complete guide for implementing the admin panel using Blazor Server with MudBlazor components.

---

## Architecture Decision

### Technology Stack
- **Blazor Server** - Real-time, server-side rendering with SignalR
- **MudBlazor 6.x** - Material Design component library
- **.NET 8** - Current LTS version
- **.NET Aspire** (Optional) - For orchestration and service discovery

### Why Blazor Server (vs WebAssembly)?

**Chosen: Blazor Server** ✅
- Real-time updates via SignalR (perfect for admin panel)
- Direct access to server-side services
- Smaller payload (no WASM download)
- Better for internal admin tools
- Easier to secure (everything server-side)

**Not Chosen: Blazor WebAssembly**
- Larger initial download
- Need to duplicate logic client-side
- More complex authentication
- Better for public-facing SPAs

---

## Implementation Steps

### Step 1: Add NuGet Packages

```bash
cd src/HnHMapperServer.Api
dotnet add package MudBlazor --version 6.11.2
```

### Step 2: Update Program.cs

```csharp
// Add Blazor services (after existing services)
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add MudBlazor services
builder.Services.AddMudServices();

// Configure MudBlazor (optional customization)
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
    config.SnackbarConfiguration.PreventDuplicates = false;
    config.SnackbarConfiguration.NewestOnTop = true;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 3000;
});

// ... existing middleware ...

// Add Blazor endpoints (before app.Run())
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
```

### Step 3: Create Component Structure

```
src/HnHMapperServer.Api/
├── Components/
│   ├── App.razor                    # Root component
│   ├── Routes.razor                 # Routing configuration
│   ├── _Imports.razor               # Global usings
│   ├── Layout/
│   │   ├── MainLayout.razor         # Main layout with sidebar
│   │   ├── MainLayout.razor.css     # Layout styles
│   │   └── NavMenu.razor            # Navigation menu
│   └── Pages/
│       ├── Login.razor              # Login page
│       ├── Index.razor              # User dashboard
│       ├── Password.razor           # Password change
│       └── Admin/
│           ├── Index.razor          # Admin dashboard
│           ├── UserEditor.razor     # User create/edit
│           └── MapEditor.razor      # Map create/edit
└── wwwroot/
    └── (MudBlazor uses CDN, no local assets needed)
```

---

## Component Examples

### App.razor (Root)
```razor
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>HnH Mapper Server</title>
    <base href="/" />
    <link href="https://fonts.googleapis.com/css?family=Roboto:300,400,500,700&display=swap" rel="stylesheet" />
    <link href="_content/MudBlazor/MudBlazor.min.css" rel="stylesheet" />
    <HeadOutlet />
</head>
<body>
    <Routes />
    <script src="_framework/blazor.web.js"></script>
    <script src="_content/MudBlazor/MudBlazor.min.js"></script>
</body>
</html>
```

### Routes.razor
```razor
<Router AppAssembly="@typeof(App).Assembly">
    <Found Context="routeData">
        <RouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)" />
        <FocusOnNavigate RouteData="@routeData" Selector="h1" />
    </Found>
    <NotFound>
        <PageTitle>Not found</PageTitle>
        <LayoutView Layout="@typeof(MainLayout)">
            <p>Sorry, there's nothing at this address.</p>
        </LayoutView>
    </NotFound>
</Router>
```

### _Imports.razor
```razor
@using System.Net.Http
@using Microsoft.AspNetCore.Components.Forms
@using Microsoft.AspNetCore.Components.Routing
@using Microsoft.AspNetCore.Components.Web
@using Microsoft.AspNetCore.Components.Authorization
@using Microsoft.JSInterop
@using MudBlazor
@using HnHMapperServer.Api.Components
@using HnHMapperServer.Api.Components.Layout
@using HnHMapperServer.Core.Models
@using HnHMapperServer.Core.Enums
@using HnHMapperServer.Services.Interfaces
```

### MainLayout.razor
```razor
@inherits LayoutComponentBase

<MudThemeProvider />
<MudDialogProvider />
<MudSnackbarProvider />

<MudLayout>
    <MudAppBar Elevation="1">
        <MudIconButton Icon="@Icons.Material.Filled.Menu" Color="Color.Inherit" Edge="Edge.Start" OnClick="@ToggleDrawer" />
        <MudText Typo="Typo.h6">HnH Auto-Mapper Server</MudText>
        <MudSpacer />
        <MudIconButton Icon="@Icons.Material.Filled.Brightness4" Color="Color.Inherit" OnClick="@ToggleDarkMode" />
    </MudAppBar>

    <MudDrawer @bind-Open="_drawerOpen" Elevation="2">
        <NavMenu />
    </MudDrawer>

    <MudMainContent>
        <MudContainer MaxWidth="MaxWidth.ExtraLarge" Class="mt-4">
            @Body
        </MudContainer>
    </MudMainContent>
</MudLayout>

@code {
    private bool _drawerOpen = true;
    private bool _darkMode = false;

    private void ToggleDrawer()
    {
        _drawerOpen = !_drawerOpen;
    }

    private void ToggleDarkMode()
    {
        _darkMode = !_darkMode;
        // TODO: Implement theme switching
    }
}
```

### NavMenu.razor
```razor
<MudNavMenu>
    <MudNavLink Href="/" Icon="@Icons.Material.Filled.Dashboard">Dashboard</MudNavLink>
    <MudNavLink Href="/password" Icon="@Icons.Material.Filled.Lock">Change Password</MudNavLink>

    <AuthorizeView Roles="Admin">
        <MudDivider Class="my-2" />
        <MudNavGroup Title="Administration" Icon="@Icons.Material.Filled.AdminPanelSettings" Expanded="true">
            <MudNavLink Href="/admin" Icon="@Icons.Material.Filled.Dashboard">Admin Dashboard</MudNavLink>
            <MudNavLink Href="/admin/users" Icon="@Icons.Material.Filled.People">Users</MudNavLink>
            <MudNavLink Href="/admin/maps" Icon="@Icons.Material.Filled.Map">Maps</MudNavLink>
            <MudNavLink Href="/admin/config" Icon="@Icons.Material.Filled.Settings">Configuration</MudNavLink>
        </MudNavGroup>
    </AuthorizeView>

    <MudDivider Class="my-2" />
    <MudNavLink Href="/logout" Icon="@Icons.Material.Filled.Logout">Logout</MudNavLink>
</MudNavMenu>
```

### Login.razor (Example)
```razor
@page "/login"
@layout EmptyLayout
@inject IAuthenticationService AuthService
@inject NavigationManager Navigation

<MudContainer MaxWidth="MaxWidth.Small" Class="d-flex align-center justify-center" Style="height: 100vh;">
    <MudPaper Elevation="4" Class="pa-8" Width="100%">
        <MudText Typo="Typo.h4" Align="Align.Center" GutterBottom="true">
            HnH Mapper Login
        </MudText>

        <EditForm Model="@_loginModel" OnValidSubmit="OnValidSubmit">
            <DataAnnotationsValidator />

            <MudTextField @bind-Value="_loginModel.Username"
                          Label="Username"
                          Variant="Variant.Outlined"
                          Required="true"
                          Class="mt-4" />

            <MudTextField @bind-Value="_loginModel.Password"
                          Label="Password"
                          Variant="Variant.Outlined"
                          InputType="InputType.Password"
                          Required="true"
                          Class="mt-4" />

            @if (!string.IsNullOrEmpty(_errorMessage))
            {
                <MudAlert Severity="Severity.Error" Class="mt-4">@_errorMessage</MudAlert>
            }

            <MudButton ButtonType="ButtonType.Submit"
                       Variant="Variant.Filled"
                       Color="Color.Primary"
                       FullWidth="true"
                       Class="mt-4">
                Login
            </MudButton>
        </EditForm>
    </MudPaper>
</MudContainer>

@code {
    private LoginModel _loginModel = new();
    private string? _errorMessage;

    private async Task OnValidSubmit()
    {
        var user = await AuthService.AuthenticateAsync(_loginModel.Username, _loginModel.Password);

        if (user != null)
        {
            // Create session and redirect
            var session = await AuthService.CreateSessionAsync(_loginModel.Username, user.Auths.Has(AuthRole.TempAdmin));
            // Set cookie via HttpContext
            Navigation.NavigateTo("/");
        }
        else
        {
            _errorMessage = "Invalid username or password";
        }
    }

    private class LoginModel
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }
}
```

### Admin Dashboard (Example)
```razor
@page "/admin"
@attribute [Authorize(Roles = "Admin")]
@inject IUserRepository UserRepository
@inject IMapRepository MapRepository

<PageTitle>Admin Dashboard</PageTitle>

<MudText Typo="Typo.h3" GutterBottom="true">Admin Dashboard</MudText>

<MudGrid>
    <MudItem xs="12" md="6">
        <MudPaper Elevation="2" Class="pa-4">
            <MudText Typo="Typo.h5" GutterBottom="true">Users</MudText>

            <MudTable Items="@_users" Hover="true" Breakpoint="Breakpoint.Sm">
                <HeaderContent>
                    <MudTh>Username</MudTh>
                    <MudTh>Actions</MudTh>
                </HeaderContent>
                <RowTemplate>
                    <MudTd DataLabel="Username">@context</MudTd>
                    <MudTd DataLabel="Actions">
                        <MudButton Href="@($"/admin/user/{context}")"
                                   Variant="Variant.Filled"
                                   Color="Color.Primary"
                                   Size="Size.Small">
                            Edit
                        </MudButton>
                    </MudTd>
                </RowTemplate>
            </MudTable>

            <MudButton Href="/admin/user/new"
                       Variant="Variant.Filled"
                       Color="Color.Success"
                       Class="mt-4">
                Add User
            </MudButton>
        </MudPaper>
    </MudItem>

    <MudItem xs="12" md="6">
        <MudPaper Elevation="2" Class="pa-4">
            <MudText Typo="Typo.h5" GutterBottom="true">Maps</MudText>

            <MudTable Items="@_maps" Hover="true" Breakpoint="Breakpoint.Sm">
                <HeaderContent>
                    <MudTh>Name</MudTh>
                    <MudTh>Hidden</MudTh>
                    <MudTh>Actions</MudTh>
                </HeaderContent>
                <RowTemplate>
                    <MudTd DataLabel="Name">@context.Name</MudTd>
                    <MudTd DataLabel="Hidden">
                        <MudChip Size="Size.Small" Color="@(context.Hidden ? Color.Error : Color.Success)">
                            @(context.Hidden ? "Hidden" : "Visible")
                        </MudChip>
                    </MudTd>
                    <MudTd DataLabel="Actions">
                        <MudButton Href="@($"/admin/map/{context.Id}")"
                                   Variant="Variant.Filled"
                                   Color="Color.Primary"
                                   Size="Size.Small">
                            Edit
                        </MudButton>
                    </MudTd>
                </RowTemplate>
            </MudTable>
        </MudPaper>
    </MudItem>
</MudGrid>

@code {
    private List<string> _users = new();
    private List<MapInfo> _maps = new();

    protected override async Task OnInitializedAsync()
    {
        _users = await UserRepository.GetAllUsernamesAsync();
        _maps = await MapRepository.GetAllMapsAsync();
    }
}
```

---

## Authentication Integration

### Create AuthenticationStateProvider

```csharp
// File: Services/ServerAuthenticationStateProvider.cs
public class ServerAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ISessionRepository _sessionRepository;

    public ServerAuthenticationStateProvider(
        IHttpContextAccessor httpContextAccessor,
        ISessionRepository sessionRepository)
    {
        _httpContextAccessor = httpContextAccessor;
        _sessionRepository = sessionRepository;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        var session = httpContext.GetSession();
        if (session == null)
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, session.Username),
        };

        foreach (var auth in session.Auths)
        {
            claims.Add(new Claim(ClaimTypes.Role, auth));
        }

        var identity = new ClaimsIdentity(claims, "Session");
        var principal = new ClaimsPrincipal(identity);

        return new AuthenticationState(principal);
    }

    public void NotifyAuthenticationStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
```

### Register in Program.cs

```csharp
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
builder.Services.AddAuthorizationCore();
```

---

## MudBlazor Component Examples

### Form with Validation
```razor
<EditForm Model="@_user" OnValidSubmit="SaveUser">
    <DataAnnotationsValidator />

    <MudTextField @bind-Value="_user.Username"
                  Label="Username"
                  Variant="Variant.Outlined"
                  For="@(() => _user.Username)" />

    <MudTextField @bind-Value="_user.Password"
                  Label="Password"
                  Variant="Variant.Outlined"
                  InputType="InputType.Password"
                  For="@(() => _user.Password)" />

    <MudButton ButtonType="ButtonType.Submit"
               Variant="Variant.Filled"
               Color="Color.Primary">
        Save
    </MudButton>
</EditForm>
```

### Data Table with Actions
```razor
<MudTable Items="@_items" Hover="true" Dense="true">
    <ToolBarContent>
        <MudText Typo="Typo.h6">Users</MudText>
        <MudSpacer />
        <MudTextField @bind-Value="_searchString"
                      Placeholder="Search"
                      Adornment="Adornment.Start"
                      AdornmentIcon="@Icons.Material.Filled.Search" />
    </ToolBarContent>
    <HeaderContent>
        <MudTh>Name</MudTh>
        <MudTh>Actions</MudTh>
    </HeaderContent>
    <RowTemplate>
        <MudTd>@context.Name</MudTd>
        <MudTd>
            <MudIconButton Icon="@Icons.Material.Filled.Edit"
                           OnClick="@(() => Edit(context))" />
            <MudIconButton Icon="@Icons.Material.Filled.Delete"
                           Color="Color.Error"
                           OnClick="@(() => Delete(context))" />
        </MudTd>
    </RowTemplate>
</MudTable>
```

### Dialog Example
```razor
@inject IDialogService DialogService

<MudButton OnClick="OpenDialog">Delete</MudButton>

@code {
    private async Task OpenDialog()
    {
        var parameters = new DialogParameters
        {
            ["ContentText"] = "Are you sure you want to delete this user?",
            ["ButtonText"] = "Delete",
            ["Color"] = Color.Error
        };

        var dialog = await DialogService.ShowAsync<ConfirmDialog>("Delete User", parameters);
        var result = await dialog.Result;

        if (!result.Canceled)
        {
            await DeleteUser();
        }
    }
}
```

### Snackbar Notifications
```razor
@inject ISnackbar Snackbar

@code {
    private void ShowSuccess()
    {
        Snackbar.Add("User saved successfully!", Severity.Success);
    }

    private void ShowError()
    {
        Snackbar.Add("Failed to save user", Severity.Error);
    }
}
```

---

## .NET Aspire Integration (Optional)

### Create AppHost Project

```bash
cd ..
dotnet new aspire-apphost -n HnHMapperServer.AppHost
cd HnHMapperServer.AppHost
dotnet add reference ../src/HnHMapperServer.Api/HnHMapperServer.Api.csproj
```

### Program.cs in AppHost

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var mapperApi = builder.AddProject<Projects.HnHMapperServer_Api>("mapper-api");

builder.Build().Run();
```

### Add ServiceDefaults to Api Project

```bash
cd ../src/HnHMapperServer.Api
dotnet add package Aspire.Microsoft.EntityFrameworkCore.Sqlite
```

---

## Migration Strategy

### Phase 1: Parallel Development
1. Keep existing AuthEndpoints.cs functional
2. Build Blazor pages alongside
3. Test Blazor pages before removing old endpoints

### Phase 2: Gradual Migration
1. Migrate login page first
2. Then dashboard
3. Then admin panel
4. Finally remove old HTML endpoints

### Phase 3: Cleanup
1. Remove inline HTML from AuthEndpoints.cs
2. Remove old endpoint registrations
3. Update documentation

---

## Benefits Summary

### Developer Experience
- ✅ Type-safe C# throughout
- ✅ IntelliSense everywhere
- ✅ Compile-time errors
- ✅ Easy debugging
- ✅ Share models/services

### User Experience
- ✅ Modern, responsive UI
- ✅ Real-time updates
- ✅ Consistent look and feel
- ✅ Better validation
- ✅ Loading indicators
- ✅ Toast notifications

### Maintainability
- ✅ Component reusability
- ✅ Clear separation of concerns
- ✅ Easy to extend
- ✅ Well-documented (MudBlazor docs)

---

**Next Steps:** See implementation in `HnHMapperServer.Api/Components/`
