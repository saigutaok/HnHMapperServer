# .NET Aspire Orchestration Setup

## Overview

The HnH Mapper now uses **.NET Aspire** for orchestration, providing:

- **Automatic Service Discovery** - Web UI automatically finds API backend
- **Built-in Telemetry** - OpenTelemetry for metrics, tracing, and logging
- **Health Checks** - Automatic health monitoring for all services
- **Developer Dashboard** - Beautiful UI to monitor all services
- **Resilience** - Automatic retry and circuit breaker patterns
- **One-Click Startup** - Start both API and Web with a single command

---

## Project Structure

```
HnHMapperServer/
├── src/
│   ├── HnHMapperServer.AppHost/         # Aspire Orchestrator
│   │   └── AppHost.cs                    # Service configuration
│   │
│   ├── HnHMapperServer.ServiceDefaults/ # Shared Configuration
│   │   └── Extensions.cs                 # Telemetry, health checks
│   │
│   ├── HnHMapperServer.Api/             # Backend API
│   │   └── Program.cs                    # Now uses ServiceDefaults
│   │
│   └── HnHMapperServer.Web/             # Frontend UI
│       └── Program.cs                    # Now uses ServiceDefaults
```

---

## How It Works

### 1. AppHost (Orchestrator)

**File:** `src/HnHMapperServer.AppHost/AppHost.cs`

```csharp
var builder = DistributedApplication.CreateBuilder(args);

// Add the API backend
var api = builder.AddProject<Projects.HnHMapperServer_Api>("api")
    .WithHttpEndpoint(port: 8080, name: "http");

// Add the Web frontend with reference to API for service discovery
builder.AddProject<Projects.HnHMapperServer_Web>("web")
    .WithHttpEndpoint(port: 5189, name: "http")
    .WithReference(api);

builder.Build().Run();
```

**What this does:**
- Defines two services: `api` and `web`
- Configures ports: API on 8080, Web on 5189
- Creates service reference so Web can discover API

### 2. ServiceDefaults (Shared Config)

**File:** `src/HnHMapperServer.ServiceDefaults/Extensions.cs`

Provides these extension methods:

- `AddServiceDefaults()` - Adds telemetry, health checks, service discovery
- `ConfigureOpenTelemetry()` - Sets up metrics and tracing
- `AddDefaultHealthChecks()` - Adds health check endpoints
- `MapDefaultEndpoints()` - Maps `/health` and `/alive` endpoints

### 3. API Integration

**File:** `src/HnHMapperServer.Api/Program.cs`

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults
builder.AddServiceDefaults();

// ... rest of configuration ...

var app = builder.Build();

// ... middleware configuration ...

// Map health check endpoints
app.MapDefaultEndpoints();

app.Run();
```

### 4. Web Integration

**File:** `src/HnHMapperServer.Web/Program.cs`

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults
builder.AddServiceDefaults();

// Add HttpClient with service discovery
builder.Services.AddHttpClient<IApiClient, ApiClient>(client =>
{
    // When running via Aspire: "http://api" is resolved automatically
    // When running standalone: Falls back to ApiBaseUrl config
    var apiBaseUrl = builder.Configuration.GetValue<string>("ApiBaseUrl");
    if (!string.IsNullOrEmpty(apiBaseUrl))
    {
        client.BaseAddress = new Uri(apiBaseUrl);
    }
    else
    {
        client.BaseAddress = new Uri("http://api");
    }
});

var app = builder.Build();

// ... middleware configuration ...

// Map health check endpoints
app.MapDefaultEndpoints();

app.Run();
```

---

## Running with Aspire

### Start Everything with One Command

```bash
cd HnHMapperServer/src/HnHMapperServer.AppHost
dotnet run
```

This starts:
- **Aspire Dashboard** - http://localhost:15001 (monitoring UI)
- **API Backend** - http://localhost:8080
- **Web Frontend** - http://localhost:5189

### Aspire Dashboard

The dashboard shows:
- **Resources** - All running services (api, web)
- **Metrics** - HTTP requests, response times, error rates
- **Traces** - Distributed tracing across services
- **Logs** - Centralized logs from all services
- **Environment** - Configuration and environment variables

---

## Service Discovery

### How It Works

**Without Aspire** (Manual Configuration):
```csharp
// Web needs hardcoded API URL
client.BaseAddress = new Uri("http://localhost:8080");
```

**With Aspire** (Automatic Discovery):
```csharp
// Web uses service name - Aspire resolves it automatically
client.BaseAddress = new Uri("http://api");
```

**Magic Happens:**
1. Web requests `http://api`
2. Aspire intercepts the request
3. Resolves "api" to actual URL (e.g., `http://localhost:8080`)
4. Request reaches API backend
5. Response flows back to Web

### Benefits

- **No hardcoded URLs** - Services find each other by name
- **Flexible deployment** - Works locally, Docker, Kubernetes
- **Dynamic scaling** - Add replicas, load balance automatically
- **Environment-agnostic** - Same code works everywhere

---

## Telemetry & Observability

### OpenTelemetry Integration

Aspire automatically instruments:

**Metrics:**
- HTTP request count
- Request duration
- Error rates
- CPU/memory usage
- Custom business metrics

**Tracing:**
- End-to-end request flows
- Service call chains
- Database queries
- External API calls
- Custom spans

**Logging:**
- Structured logs from all services
- Log correlation with traces
- Centralized log aggregation

### Viewing Telemetry

1. Start AppHost: `dotnet run --project src/HnHMapperServer.AppHost`
2. Open dashboard: http://localhost:15001
3. Navigate to:
   - **Resources** - Service status and URLs
   - **Metrics** - Real-time performance graphs
   - **Traces** - Request flow visualization
   - **Logs** - Searchable log stream

---

## Health Checks

### Endpoints

Both API and Web expose:

- `GET /health` - Overall health (all checks must pass)
- `GET /alive` - Liveness check (basic responsiveness)

### Usage

```bash
# Check API health
curl http://localhost:8080/health

# Check Web health
curl http://localhost:5189/health

# Check liveness
curl http://localhost:8080/alive
```

### Custom Health Checks

Add custom health checks in ServiceDefaults:

```csharp
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy())
    .AddDbContextCheck<ApplicationDbContext>("database")
    .AddCheck("external-api", () =>
    {
        // Custom health check logic
        return HealthCheckResult.Healthy();
    });
```

---

## Development Workflow

### 1. Start with Aspire (Recommended)

```bash
cd HnHMapperServer/src/HnHMapperServer.AppHost
dotnet run
```

**Pros:**
- Single command starts everything
- Service discovery works
- Telemetry dashboard available
- Realistic production-like environment

### 2. Start Services Individually (Fallback)

**Terminal 1 - API:**
```bash
cd HnHMapperServer/src/HnHMapperServer.Api
dotnet run
```

**Terminal 2 - Web:**
```bash
cd HnHMapperServer/src/HnHMapperServer.Web
dotnet run
```

**Pros:**
- Easier debugging
- Faster iteration
- Less resource usage

**Cons:**
- Need to configure ApiBaseUrl manually
- No service discovery
- No centralized telemetry

### 3. Debug in Visual Studio / VS Code

**Visual Studio:**
1. Open `HnHMapperServer.sln`
2. Set `HnHMapperServer.AppHost` as startup project
3. Press F5

**VS Code:**
1. Open workspace folder
2. Add launch configuration for AppHost
3. Start debugging

---

## Configuration

### Aspire Configuration

**File:** `src/HnHMapperServer.AppHost/appsettings.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.Hosting.Lifetime": "Information",
      "Aspire": "Information"
    }
  }
}
```

### API Configuration

**File:** `src/HnHMapperServer.Api/appsettings.json`

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

### Web Configuration

**File:** `src/HnHMapperServer.Web/appsettings.json`

```json
{
  "ApiBaseUrl": "",  // Empty = use service discovery
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

**Standalone mode:**
```json
{
  "ApiBaseUrl": "http://localhost:8080",  // Explicit URL
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

---

## Production Deployment

### Option 1: Docker Containers (Aspire)

Aspire can generate Docker Compose files:

```bash
cd HnHMapperServer
dotnet publish src/HnHMapperServer.AppHost -c Release

# Generates docker-compose.yml with all services
docker-compose up
```

### Option 2: Kubernetes (Aspire)

Aspire can generate Kubernetes manifests:

```bash
# Install Aspire Kubernetes tooling
dotnet tool install -g aspirate

# Generate Kubernetes manifests
cd HnHMapperServer
aspirate generate

# Deploy to Kubernetes
kubectl apply -f aspire-manifest.yaml
```

### Option 3: Azure Container Apps (Aspire)

Aspire integrates with Azure:

```bash
# Publish to Azure Container Apps
azd up
```

### Option 4: Manual Deployment (Standalone)

Deploy API and Web independently:

```bash
# Publish API
dotnet publish src/HnHMapperServer.Api -c Release -o ./publish/api

# Publish Web
dotnet publish src/HnHMapperServer.Web -c Release -o ./publish/web

# Configure ApiBaseUrl in Web's appsettings.json
# Deploy both to servers
```

---

## Troubleshooting

### Aspire Dashboard Not Opening

**Problem:** Dashboard doesn't open automatically

**Solution:**
1. Check console output for dashboard URL
2. Manually navigate to http://localhost:15001
3. Check if port 15001 is already in use

### Service Discovery Not Working

**Problem:** Web can't find API

**Solution:**
1. Ensure `.WithReference(api)` in AppHost.cs
2. Verify ServiceDefaults is referenced by both projects
3. Check HttpClient configuration uses `http://api`
4. Restart AppHost

### Port Conflicts

**Problem:** Port already in use

**Solution:**
1. Change ports in AppHost.cs:
   ```csharp
   .WithHttpEndpoint(port: 8081, name: "http")
   ```
2. Or let Aspire assign random ports:
   ```csharp
   .WithHttpEndpoint(name: "http")  // Random port
   ```

### Build Errors

**Problem:** "AddServiceDefaults" not found

**Solution:**
1. Ensure ServiceDefaults project targets .NET 8.0
2. Verify project references are correct
3. Clean and rebuild solution:
   ```bash
   dotnet clean
   dotnet build
   ```

---

## Benefits Summary

✅ **Simplified Development**
- One command starts everything
- Automatic service discovery
- No manual URL configuration

✅ **Better Observability**
- Built-in telemetry dashboard
- Distributed tracing
- Centralized logging

✅ **Production Ready**
- Health checks included
- Resilience patterns built-in
- Deploy to containers or Kubernetes

✅ **Developer Experience**
- Fast inner loop
- Easy debugging
- Rich tooling support

---

## Next Steps

### Immediate

- [x] Aspire orchestration working
- [x] Service discovery configured
- [x] Health checks enabled
- [ ] Test end-to-end flow

### Future Enhancements

- [ ] Add database health check
- [ ] Configure custom telemetry metrics
- [ ] Set up log filtering
- [ ] Add resilience policies (retry, circuit breaker)
- [ ] Configure distributed tracing sampling
- [ ] Add Azure Application Insights integration

---

## Resources

- **Aspire Docs:** https://learn.microsoft.com/dotnet/aspire
- **Service Discovery:** https://learn.microsoft.com/dotnet/aspire/service-discovery
- **OpenTelemetry:** https://opentelemetry.io
- **Health Checks:** https://learn.microsoft.com/aspnet/core/host-and-deploy/health-checks

---

**Last Updated:** 2025-10-25
