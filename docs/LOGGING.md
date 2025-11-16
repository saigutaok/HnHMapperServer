# Logging Configuration

This project uses **Serilog** for structured logging with file and console outputs.

## Log File Locations

All logs are written to rolling daily log files:

- **API Logs**: `HnHMapperServer/src/HnHMapperServer.Api/logs/api-YYYYMMDD.log`
- **Web Logs**: `HnHMapperServer/src/HnHMapperServer.Web/logs/web-YYYYMMDD.log`

## Log Retention

- Logs are rolled daily (new file each day)
- Last 7 days of logs are retained
- Older logs are automatically deleted

## Log Format

Logs use a detailed format with timestamp, level, source context, and message:

```
[2025-10-25 12:34:56.789 +00:00 INF] AuthEndpoints: Login attempt for user: admin from ::1
[2025-10-25 12:34:56.892 +00:00 INF] AuthEndpoints: Authentication successful for user: admin
[2025-10-25 12:34:56.903 +00:00 INF] AuthEndpoints: Created session abc123... for user: admin
```

## Reading Logs

### Using CLI Tools

**View latest API logs:**
```bash
cat HnHMapperServer/src/HnHMapperServer.Api/logs/api-$(date +%Y%m%d).log
```

**View latest Web logs:**
```bash
cat HnHMapperServer/src/HnHMapperServer.Web/logs/web-$(date +%Y%m%d).log
```

**Tail logs in real-time (Windows PowerShell):**
```powershell
Get-Content HnHMapperServer\src\HnHMapperServer.Api\logs\api-$(Get-Date -Format "yyyyMMdd").log -Wait -Tail 50
```

**Tail logs in real-time (Linux/Mac):**
```bash
tail -f HnHMapperServer/src/HnHMapperServer.Api/logs/api-$(date +%Y%m%d).log
```

### Using Text Editors

Simply open the log files in any text editor:
- Visual Studio Code
- Notepad++
- Sublime Text
- etc.

## Log Levels

Configured log levels:
- **Default**: Information
- **Microsoft.AspNetCore**: Warning (reduces noise)
- **System**: Warning (reduces noise)

## Configuration

Logging is configured in `appsettings.json`:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft.AspNetCore": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/api-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7,
          "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"
        }
      }
    ]
  }
}
```

## Changing Log Levels

To see more detailed logs, change `"Default": "Information"` to:
- `"Debug"` - Very detailed logs
- `"Trace"` - Everything including framework internals
- `"Warning"` - Only warnings and errors
- `"Error"` - Only errors

Then restart the application.

## Troubleshooting

### No Log Files Created

1. Check that the `logs` directory exists:
   ```bash
   ls -la HnHMapperServer/src/HnHMapperServer.Api/logs/
   ```

2. Check file permissions - the application must be able to write to the logs directory

3. Check application startup logs in console for Serilog initialization errors

### Log Files Too Large

Reduce log retention or increase log level to Warning:
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Warning"
    },
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "retainedFileCountLimit": 3
        }
      }
    ]
  }
}
```

## Structured Logging Examples

When writing log messages, use structured logging:

```csharp
// Good - structured
_logger.LogInformation("User {Username} logged in from {IpAddress}", username, ipAddress);

// Bad - string interpolation
_logger.LogInformation($"User {username} logged in from {ipAddress}");
```

Structured logging allows better searching and filtering of logs.
