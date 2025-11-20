using HnHMapperServer.Core.DTOs;
using HnHMapperServer.Infrastructure.Data;
using HnHMapperServer.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HnHMapperServer.Services.Services;

/// <summary>
/// Implementation of timer service.
/// Manages creation, retrieval, and lifecycle of timers (both marker and standalone).
/// </summary>
public class TimerService : ITimerService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<TimerService> _logger;

    public TimerService(
        ApplicationDbContext db,
        ILogger<TimerService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Create a new timer.
    /// </summary>
    public async Task<TimerDto> CreateAsync(CreateTimerDto dto, string userId, string tenantId)
    {
        var entity = new TimerEntity
        {
            TenantId = tenantId,
            UserId = userId,
            Type = dto.Type,
            MarkerId = dto.MarkerId,
            CustomMarkerId = dto.CustomMarkerId,
            Title = dto.Title,
            Description = dto.Description,
            ReadyAt = dto.ReadyAt,
            CreatedAt = DateTime.UtcNow,
            IsCompleted = false,
            NotificationSent = false,
            PreExpiryWarningSent = false
        };

        _db.Timers.Add(entity);
        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "Created timer {Id} of type {Type} for tenant {TenantId}, ready at {ReadyAt}",
            entity.Id, entity.Type, entity.TenantId, entity.ReadyAt);

        return MapToDto(entity);
    }

    /// <summary>
    /// Get a timer by ID.
    /// </summary>
    public async Task<TimerDto?> GetByIdAsync(int id)
    {
        var entity = await _db.Timers.FindAsync(id);
        return entity == null ? null : MapToDto(entity);
    }

    /// <summary>
    /// Get timers for a specific tenant.
    /// </summary>
    public async Task<List<TimerDto>> GetTenantTimersAsync(
        string tenantId,
        bool includeCompleted = false,
        int limit = 100)
    {
        var query = _db.Timers.Where(t => t.TenantId == tenantId);

        if (!includeCompleted)
        {
            query = query.Where(t => !t.IsCompleted);
        }

        var entities = await query
            .OrderBy(t => t.ReadyAt)
            .Take(limit)
            .ToListAsync();

        return entities.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Get timers for a specific user.
    /// </summary>
    public async Task<List<TimerDto>> GetUserTimersAsync(
        string userId,
        bool includeCompleted = false,
        int limit = 100)
    {
        var query = _db.Timers.Where(t => t.UserId == userId);

        if (!includeCompleted)
        {
            query = query.Where(t => !t.IsCompleted);
        }

        var entities = await query
            .OrderBy(t => t.ReadyAt)
            .Take(limit)
            .ToListAsync();

        return entities.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Get timers for a specific marker.
    /// </summary>
    public async Task<List<TimerDto>> GetMarkerTimersAsync(int markerId)
    {
        var entities = await _db.Timers
            .Where(t => t.MarkerId == markerId && !t.IsCompleted)
            .OrderBy(t => t.ReadyAt)
            .ToListAsync();

        return entities.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Get timers for a specific custom marker.
    /// </summary>
    public async Task<List<TimerDto>> GetCustomMarkerTimersAsync(int customMarkerId)
    {
        var entities = await _db.Timers
            .Where(t => t.CustomMarkerId == customMarkerId && !t.IsCompleted)
            .OrderBy(t => t.ReadyAt)
            .ToListAsync();

        return entities.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Query timers with filtering.
    /// </summary>
    public async Task<List<TimerDto>> QueryAsync(TimerQuery query)
    {
        var dbQuery = _db.Timers.AsQueryable();

        // Apply filters
        if (query.TenantId != null)
            dbQuery = dbQuery.Where(t => t.TenantId == query.TenantId);

        if (query.UserId != null)
            dbQuery = dbQuery.Where(t => t.UserId == query.UserId);

        if (query.Type != null)
            dbQuery = dbQuery.Where(t => t.Type == query.Type);

        if (query.MarkerId != null)
            dbQuery = dbQuery.Where(t => t.MarkerId == query.MarkerId);

        if (query.CustomMarkerId != null)
            dbQuery = dbQuery.Where(t => t.CustomMarkerId == query.CustomMarkerId);

        if (query.IsCompleted != null)
            dbQuery = dbQuery.Where(t => t.IsCompleted == query.IsCompleted.Value);

        if (query.IsReady != null)
        {
            var now = DateTime.UtcNow;
            if (query.IsReady.Value)
                dbQuery = dbQuery.Where(t => t.ReadyAt <= now);
            else
                dbQuery = dbQuery.Where(t => t.ReadyAt > now);
        }

        // Execute query with pagination
        var entities = await dbQuery
            .OrderBy(t => t.ReadyAt)
            .Skip(query.Offset)
            .Take(query.Limit)
            .ToListAsync();

        return entities.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Update a timer.
    /// </summary>
    public async Task<TimerDto?> UpdateAsync(int id, UpdateTimerDto dto, string userId)
    {
        var entity = await _db.Timers.FindAsync(id);
        if (entity == null)
            return null;

        // Authorization check: user can only update their own timers
        if (entity.UserId != userId)
            return null;

        // Update fields if provided
        if (dto.Title != null)
            entity.Title = dto.Title;

        if (dto.Description != null)
            entity.Description = dto.Description;

        if (dto.ReadyAt != null)
        {
            entity.ReadyAt = dto.ReadyAt.Value;
            // Reset notification flags if time changed
            entity.NotificationSent = false;
            entity.PreExpiryWarningSent = false;
        }

        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "Updated timer {Id} for user {UserId}",
            id, userId);

        return MapToDto(entity);
    }

    /// <summary>
    /// Delete a timer.
    /// </summary>
    public async Task<bool> DeleteAsync(int id, string userId)
    {
        var entity = await _db.Timers.FindAsync(id);
        if (entity == null)
            return false;

        // Authorization check
        if (entity.UserId != userId)
            return false;

        _db.Timers.Remove(entity);
        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "Deleted timer {Id} for user {UserId}",
            id, userId);

        return true;
    }

    /// <summary>
    /// Complete a timer manually.
    /// </summary>
    public async Task<bool> CompleteAsync(int id, string userId)
    {
        var entity = await _db.Timers.FindAsync(id);
        if (entity == null)
            return false;

        // Authorization check
        if (entity.UserId != userId)
            return false;

        entity.IsCompleted = true;

        // Add to history
        var history = new TimerHistoryEntity
        {
            TimerId = entity.Id,
            TenantId = entity.TenantId,
            CompletedAt = DateTime.UtcNow,
            Duration = (int)(DateTime.UtcNow - entity.CreatedAt).TotalMinutes,
            Type = entity.Type,
            MarkerId = entity.MarkerId,
            CustomMarkerId = entity.CustomMarkerId,
            Title = entity.Title
        };

        _db.TimerHistory.Add(history);
        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "Completed timer {Id} for user {UserId}",
            id, userId);

        return true;
    }

    /// <summary>
    /// Get timers that need processing (for background service).
    /// </summary>
    public async Task<List<TimerEntity>> GetTimersNeedingProcessingAsync(string tenantId)
    {
        var now = DateTime.UtcNow;

        // Get timers that are:
        // - Not completed
        // - Either expired or nearing expiry (within pre-warning time)
        var timers = await _db.Timers
            .IgnoreQueryFilters() // Required for background service (no HttpContext)
            .Where(t => t.TenantId == tenantId && !t.IsCompleted)
            .Where(t => t.ReadyAt <= now.AddMinutes(5)) // Include timers within 5 minutes of expiry
            .ToListAsync();

        return timers;
    }

    /// <summary>
    /// Mark a timer as having sent its notification.
    /// </summary>
    public async Task MarkNotificationSentAsync(int id)
    {
        var entity = await _db.Timers.FindAsync(id);
        if (entity != null)
        {
            entity.NotificationSent = true;
            entity.IsCompleted = true;

            // Add to history
            var history = new TimerHistoryEntity
            {
                TimerId = entity.Id,
                TenantId = entity.TenantId,
                CompletedAt = DateTime.UtcNow,
                Duration = (int)(DateTime.UtcNow - entity.CreatedAt).TotalMinutes,
                Type = entity.Type,
                MarkerId = entity.MarkerId,
                CustomMarkerId = entity.CustomMarkerId,
                Title = entity.Title
            };

            _db.TimerHistory.Add(history);
            await _db.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Mark a timer as having sent its pre-expiry warning.
    /// </summary>
    public async Task MarkPreExpiryWarningSentAsync(int id)
    {
        var entity = await _db.Timers.FindAsync(id);
        if (entity != null)
        {
            entity.PreExpiryWarningSent = true;
            await _db.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Get timer history for a tenant.
    /// </summary>
    public async Task<List<TimerHistoryDto>> GetHistoryAsync(string tenantId, int limit = 100)
    {
        var entities = await _db.TimerHistory
            .Where(h => h.TenantId == tenantId)
            .OrderByDescending(h => h.CompletedAt)
            .Take(limit)
            .ToListAsync();

        return entities.Select(MapToHistoryDto).ToList();
    }

    /// <summary>
    /// Get timer history for a specific marker.
    /// </summary>
    public async Task<List<TimerHistoryDto>> GetMarkerHistoryAsync(int markerId)
    {
        var entities = await _db.TimerHistory
            .Where(h => h.MarkerId == markerId)
            .OrderByDescending(h => h.CompletedAt)
            .ToListAsync();

        return entities.Select(MapToHistoryDto).ToList();
    }

    /// <summary>
    /// Map TimerEntity to TimerDto.
    /// </summary>
    private static TimerDto MapToDto(TimerEntity entity)
    {
        var now = DateTime.UtcNow;
        var timeRemaining = (entity.ReadyAt - now).TotalSeconds;

        return new TimerDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            UserId = entity.UserId,
            Type = entity.Type,
            MarkerId = entity.MarkerId,
            CustomMarkerId = entity.CustomMarkerId,
            Title = entity.Title,
            Description = entity.Description,
            ReadyAt = entity.ReadyAt,
            CreatedAt = entity.CreatedAt,
            IsCompleted = entity.IsCompleted,
            NotificationSent = entity.NotificationSent,
            TimeRemainingSeconds = (long)timeRemaining,
            IsReady = timeRemaining <= 0
        };
    }

    /// <summary>
    /// Map TimerEntity to TimerEventDto (for SSE).
    /// </summary>
    public static TimerEventDto MapToEventDto(TimerEntity entity)
    {
        return new TimerEventDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            UserId = entity.UserId,
            Type = entity.Type,
            MarkerId = entity.MarkerId,
            CustomMarkerId = entity.CustomMarkerId,
            Title = entity.Title,
            ReadyAt = entity.ReadyAt,
            CreatedAt = entity.CreatedAt,
            IsCompleted = entity.IsCompleted
        };
    }

    /// <summary>
    /// Map TimerHistoryEntity to TimerHistoryDto.
    /// </summary>
    private static TimerHistoryDto MapToHistoryDto(TimerHistoryEntity entity)
    {
        return new TimerHistoryDto
        {
            Id = entity.Id,
            TimerId = entity.TimerId,
            TenantId = entity.TenantId,
            CompletedAt = entity.CompletedAt,
            Duration = entity.Duration,
            Type = entity.Type,
            MarkerId = entity.MarkerId,
            CustomMarkerId = entity.CustomMarkerId,
            Title = entity.Title
        };
    }
}
