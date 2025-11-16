using HnHMapperServer.Core.DTOs;
using HnHMapperServer.Core.Interfaces;
using HnHMapperServer.Core.Models;
using HnHMapperServer.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace HnHMapperServer.Services.Services;

/// <summary>
/// Service implementation for ping operations with validation and rate limiting
/// </summary>
public class PingService : IPingService
{
    private readonly IPingRepository _repository;
    private readonly ITenantContextAccessor _tenantContext;
    private readonly ILogger<PingService> _logger;

    // Rate limiting: Max 5 active pings per user
    private const int MaxPingsPerUser = 5;
    private const int PingDurationSeconds = 60;

    public PingService(
        IPingRepository repository,
        ITenantContextAccessor tenantContext,
        ILogger<PingService> logger)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    /// <summary>
    /// Get all active (non-expired) pings for the current tenant
    /// </summary>
    public async Task<List<PingEventDto>> GetActiveForTenantAsync()
    {
        var pings = await _repository.GetActiveForTenantAsync();
        var tenantId = _tenantContext.GetRequiredTenantId();

        return pings.Select(p => new PingEventDto
        {
            Id = p.Id,
            MapId = p.MapId,
            CoordX = p.CoordX,
            CoordY = p.CoordY,
            X = p.X,
            Y = p.Y,
            CreatedBy = p.CreatedBy,
            CreatedAt = p.CreatedAt,
            ExpiresAt = p.ExpiresAt,
            TenantId = tenantId
        }).ToList();
    }

    /// <summary>
    /// Create a new ping with rate limiting (max 5 active pings per user)
    /// </summary>
    public async Task<PingEventDto> CreateAsync(CreatePingDto dto, string currentUsername)
    {
        // Validate coordinates
        ValidateCoordinates(dto);

        // Check rate limit: max 5 active pings per user
        var activeCount = await _repository.GetActiveCountByUserAsync(currentUsername);
        if (activeCount >= MaxPingsPerUser)
        {
            _logger.LogWarning("User {Username} attempted to create ping but has reached limit of {MaxPings}",
                currentUsername, MaxPingsPerUser);
            throw new InvalidOperationException($"You can have a maximum of {MaxPingsPerUser} active pings at a time. Please wait for existing pings to expire.");
        }

        // Create ping with 60-second expiration
        var now = DateTime.UtcNow;
        var ping = new Ping
        {
            MapId = dto.MapId,
            CoordX = dto.CoordX,
            CoordY = dto.CoordY,
            X = dto.X,
            Y = dto.Y,
            CreatedBy = currentUsername,
            CreatedAt = now,
            ExpiresAt = now.AddSeconds(PingDurationSeconds)
        };

        var created = await _repository.CreateAsync(ping);
        var tenantId = _tenantContext.GetRequiredTenantId();

        _logger.LogInformation("User {Username} created ping {PingId} on map {MapId} at ({CoordX},{CoordY},{X},{Y})",
            currentUsername, created.Id, dto.MapId, dto.CoordX, dto.CoordY, dto.X, dto.Y);

        return new PingEventDto
        {
            Id = created.Id,
            MapId = created.MapId,
            CoordX = created.CoordX,
            CoordY = created.CoordY,
            X = created.X,
            Y = created.Y,
            CreatedBy = created.CreatedBy,
            CreatedAt = created.CreatedAt,
            ExpiresAt = created.ExpiresAt,
            TenantId = tenantId
        };
    }

    /// <summary>
    /// Delete a specific ping by ID
    /// </summary>
    public async Task DeleteAsync(int id)
    {
        await _repository.DeleteAsync(id);
        _logger.LogInformation("Deleted ping {PingId}", id);
    }

    /// <summary>
    /// Delete all expired pings and return them with tenant IDs
    /// </summary>
    public async Task<List<(int Id, string TenantId)>> DeleteExpiredAsync()
    {
        var expired = await _repository.DeleteExpiredAsync();
        if (expired.Any())
        {
            _logger.LogDebug("Deleted {Count} expired pings", expired.Count);
        }
        return expired;
    }

    /// <summary>
    /// Validate coordinate ranges
    /// </summary>
    private static void ValidateCoordinates(CreatePingDto dto)
    {
        if (dto.X < 0 || dto.X > 100)
        {
            throw new ArgumentException($"X coordinate must be between 0 and 100. Got: {dto.X}");
        }

        if (dto.Y < 0 || dto.Y > 100)
        {
            throw new ArgumentException($"Y coordinate must be between 0 and 100. Got: {dto.Y}");
        }
    }
}
