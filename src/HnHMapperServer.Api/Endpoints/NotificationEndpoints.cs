using System.Security.Claims;
using HnHMapperServer.Core.DTOs;
using HnHMapperServer.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HnHMapperServer.Api.Endpoints;

/// <summary>
/// Notification API endpoints
/// </summary>
public static class NotificationEndpoints
{
    public static void MapNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/notifications")
            .RequireAuthorization(); // Require authentication for all notification APIs

        // Get user's notifications
        group.MapGet("", GetNotifications);

        // Get notification by ID
        group.MapGet("{id:int}", GetNotificationById);

        // Get unread count
        group.MapGet("unread/count", GetUnreadCount);

        // Mark notification as read
        group.MapPut("{id:int}/read", MarkAsRead);

        // Mark all notifications as read
        group.MapPut("read-all", MarkAllAsRead);

        // Dismiss notification
        group.MapDelete("{id:int}", DismissNotification);

        // Delete all read notifications
        group.MapDelete("read", DeleteAllRead);
    }

    /// <summary>
    /// Get notifications for the current user
    /// </summary>
    private static async Task<IResult> GetNotifications(
        HttpContext context,
        [FromQuery] bool includeRead,
        [FromQuery] int limit,
        INotificationService notificationService)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        try
        {
            var notifications = await notificationService.GetUserNotificationsAsync(
                userId,
                includeRead,
                limit > 0 ? limit : 50);

            return Results.Json(notifications);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    /// <summary>
    /// Get a single notification by ID
    /// </summary>
    private static async Task<IResult> GetNotificationById(
        HttpContext context,
        int id,
        INotificationService notificationService)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        try
        {
            var notification = await notificationService.GetByIdAsync(id);
            if (notification == null)
            {
                return Results.NotFound(new { error = $"Notification {id} not found" });
            }

            // Check authorization - user can only access their own notifications
            if (notification.UserId != null && notification.UserId != userId)
            {
                return Results.Forbid();
            }

            return Results.Json(notification);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    /// <summary>
    /// Get unread notification count
    /// </summary>
    private static async Task<IResult> GetUnreadCount(
        HttpContext context,
        INotificationService notificationService)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        try
        {
            var count = await notificationService.GetUnreadCountAsync(userId);
            return Results.Json(new { count });
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    /// <summary>
    /// Mark a notification as read
    /// </summary>
    private static async Task<IResult> MarkAsRead(
        HttpContext context,
        int id,
        INotificationService notificationService,
        IUpdateNotificationService updateNotificationService)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        try
        {
            var success = await notificationService.MarkAsReadAsync(id, userId);
            if (!success)
            {
                return Results.NotFound(new { error = $"Notification {id} not found or access denied" });
            }

            // Broadcast SSE event
            updateNotificationService.NotifyNotificationRead(id);

            return Results.Ok(new { success = true });
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    /// <summary>
    /// Mark all notifications as read for the current user
    /// </summary>
    private static async Task<IResult> MarkAllAsRead(
        HttpContext context,
        INotificationService notificationService)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        try
        {
            var count = await notificationService.MarkAllAsReadAsync(userId);
            return Results.Ok(new { count });
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    /// <summary>
    /// Dismiss (delete) a notification
    /// </summary>
    private static async Task<IResult> DismissNotification(
        HttpContext context,
        int id,
        INotificationService notificationService,
        IUpdateNotificationService updateNotificationService)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        try
        {
            var success = await notificationService.DismissAsync(id, userId);
            if (!success)
            {
                return Results.NotFound(new { error = $"Notification {id} not found or access denied" });
            }

            // Broadcast SSE event
            updateNotificationService.NotifyNotificationDismissed(id);

            return Results.Ok(new { success = true });
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    /// <summary>
    /// Delete all read notifications for the current user
    /// </summary>
    private static async Task<IResult> DeleteAllRead(
        HttpContext context,
        INotificationService notificationService)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        try
        {
            var count = await notificationService.DeleteAllReadAsync(userId);
            return Results.Ok(new { count });
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }
}
