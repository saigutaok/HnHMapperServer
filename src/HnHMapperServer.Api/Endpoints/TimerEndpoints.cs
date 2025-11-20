using System.Security.Claims;
using HnHMapperServer.Core.DTOs;
using HnHMapperServer.Services.Interfaces;
using HnHMapperServer.Services.Services;
using Microsoft.AspNetCore.Mvc;

namespace HnHMapperServer.Api.Endpoints;

/// <summary>
/// Timer API endpoints
/// </summary>
public static class TimerEndpoints
{
    public static void MapTimerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/timers")
            .RequireAuthorization(); // Require authentication for all timer APIs

        // Create a new timer
        group.MapPost("", CreateTimer);

        // Get all timers for the current user/tenant
        group.MapGet("", GetTimers);

        // Get a specific timer
        group.MapGet("{id:int}", GetTimerById);

        // Update a timer
        group.MapPut("{id:int}", UpdateTimer);

        // Delete a timer
        group.MapDelete("{id:int}", DeleteTimer);

        // Complete a timer manually
        group.MapPost("{id:int}/complete", CompleteTimer);

        // Get timer history
        group.MapGet("history", GetTimerHistory);

        // Get timer history for a specific marker
        group.MapGet("history/marker/{markerId:int}", GetMarkerTimerHistory);
    }

    /// <summary>
    /// Create a new timer
    /// </summary>
    private static async Task<IResult> CreateTimer(
        HttpContext context,
        [FromBody] CreateTimerDto dto,
        ITimerService timerService,
        IUpdateNotificationService updateNotificationService)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var tenantId = context.Items["TenantId"] as string;

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(tenantId))
            return Results.Unauthorized();

        // Validation
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            return Results.BadRequest(new { error = "Title is required" });
        }

        if (dto.ReadyAt <= DateTime.UtcNow)
        {
            return Results.BadRequest(new { error = "ReadyAt must be in the future" });
        }

        try
        {
            var timer = await timerService.CreateAsync(dto, userId, tenantId);

            // Broadcast SSE event
            var timerEvent = TimerService.MapToEventDto(new Infrastructure.Data.TimerEntity
            {
                Id = timer.Id,
                TenantId = timer.TenantId,
                UserId = timer.UserId,
                Type = timer.Type,
                MarkerId = timer.MarkerId,
                CustomMarkerId = timer.CustomMarkerId,
                Title = timer.Title,
                ReadyAt = timer.ReadyAt,
                CreatedAt = timer.CreatedAt,
                IsCompleted = timer.IsCompleted
            });
            updateNotificationService.NotifyTimerCreated(timerEvent);

            return Results.Created($"/api/timers/{timer.Id}", timer);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    /// <summary>
    /// Get timers for the current user/tenant
    /// </summary>
    private static async Task<IResult> GetTimers(
        HttpContext context,
        [FromQuery] bool includeCompleted,
        [FromQuery] int limit,
        [FromQuery] string? type,
        ITimerService timerService)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var tenantId = context.Items["TenantId"] as string;

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(tenantId))
            return Results.Unauthorized();

        try
        {
            List<TimerDto> timers;

            if (!string.IsNullOrEmpty(type))
            {
                // Query with type filter
                timers = await timerService.QueryAsync(new TimerQuery
                {
                    TenantId = tenantId,
                    Type = type,
                    IsCompleted = includeCompleted ? null : false,
                    Limit = limit > 0 ? limit : 100
                });
            }
            else
            {
                // Get all tenant timers
                timers = await timerService.GetTenantTimersAsync(
                    tenantId,
                    includeCompleted,
                    limit > 0 ? limit : 100);
            }

            return Results.Json(timers);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    /// <summary>
    /// Get a specific timer by ID
    /// </summary>
    private static async Task<IResult> GetTimerById(
        HttpContext context,
        int id,
        ITimerService timerService)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        try
        {
            var timer = await timerService.GetByIdAsync(id);
            if (timer == null)
            {
                return Results.NotFound(new { error = $"Timer {id} not found" });
            }

            // Check authorization - user can only access timers in their tenant
            // (tenant filter is already applied by EF Core global query filter)

            return Results.Json(timer);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    /// <summary>
    /// Update a timer
    /// </summary>
    private static async Task<IResult> UpdateTimer(
        HttpContext context,
        int id,
        [FromBody] UpdateTimerDto dto,
        ITimerService timerService,
        IUpdateNotificationService updateNotificationService)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        // Validation
        if (dto.ReadyAt.HasValue && dto.ReadyAt.Value <= DateTime.UtcNow)
        {
            return Results.BadRequest(new { error = "ReadyAt must be in the future" });
        }

        try
        {
            var timer = await timerService.UpdateAsync(id, dto, userId);
            if (timer == null)
            {
                return Results.NotFound(new { error = $"Timer {id} not found or access denied" });
            }

            // Broadcast SSE event
            var timerEvent = TimerService.MapToEventDto(new Infrastructure.Data.TimerEntity
            {
                Id = timer.Id,
                TenantId = timer.TenantId,
                UserId = timer.UserId,
                Type = timer.Type,
                MarkerId = timer.MarkerId,
                CustomMarkerId = timer.CustomMarkerId,
                Title = timer.Title,
                ReadyAt = timer.ReadyAt,
                CreatedAt = timer.CreatedAt,
                IsCompleted = timer.IsCompleted
            });
            updateNotificationService.NotifyTimerUpdated(timerEvent);

            return Results.Ok(timer);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    /// <summary>
    /// Delete a timer
    /// </summary>
    private static async Task<IResult> DeleteTimer(
        HttpContext context,
        int id,
        ITimerService timerService,
        IUpdateNotificationService updateNotificationService)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        try
        {
            var success = await timerService.DeleteAsync(id, userId);
            if (!success)
            {
                return Results.NotFound(new { error = $"Timer {id} not found or access denied" });
            }

            // Broadcast SSE event
            updateNotificationService.NotifyTimerDeleted(id);

            return Results.Ok(new { success = true });
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    /// <summary>
    /// Complete a timer manually
    /// </summary>
    private static async Task<IResult> CompleteTimer(
        HttpContext context,
        int id,
        ITimerService timerService,
        IUpdateNotificationService updateNotificationService)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        try
        {
            var success = await timerService.CompleteAsync(id, userId);
            if (!success)
            {
                return Results.NotFound(new { error = $"Timer {id} not found or access denied" });
            }

            // Broadcast SSE event (timer completed)
            // Note: We need to get the timer details to broadcast the event
            var timer = await timerService.GetByIdAsync(id);
            if (timer != null)
            {
                var timerEvent = TimerService.MapToEventDto(new Infrastructure.Data.TimerEntity
                {
                    Id = timer.Id,
                    TenantId = timer.TenantId,
                    UserId = timer.UserId,
                    Type = timer.Type,
                    MarkerId = timer.MarkerId,
                    CustomMarkerId = timer.CustomMarkerId,
                    Title = timer.Title,
                    ReadyAt = timer.ReadyAt,
                    CreatedAt = timer.CreatedAt,
                    IsCompleted = true
                });
                timerEvent.CompletedAt = DateTime.UtcNow;
                updateNotificationService.NotifyTimerCompleted(timerEvent);
            }

            return Results.Ok(new { success = true });
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    /// <summary>
    /// Get timer history for the current tenant
    /// </summary>
    private static async Task<IResult> GetTimerHistory(
        HttpContext context,
        [FromQuery] int limit,
        ITimerService timerService)
    {
        var tenantId = context.Items["TenantId"] as string;
        if (string.IsNullOrEmpty(tenantId))
            return Results.Unauthorized();

        try
        {
            var history = await timerService.GetHistoryAsync(tenantId, limit > 0 ? limit : 100);
            return Results.Json(history);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    /// <summary>
    /// Get timer history for a specific marker
    /// </summary>
    private static async Task<IResult> GetMarkerTimerHistory(
        HttpContext context,
        int markerId,
        ITimerService timerService)
    {
        if (!context.User.Identity?.IsAuthenticated ?? true)
            return Results.Unauthorized();

        try
        {
            var history = await timerService.GetMarkerHistoryAsync(markerId);
            return Results.Json(history);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }
}
