using System.Security.Claims;
using HnHMapperServer.Core.DTOs;
using HnHMapperServer.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HnHMapperServer.Api.Endpoints;

/// <summary>
/// Ping API endpoints for temporary map markers
/// </summary>
public static class PingEndpoints
{
    public static void MapPingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/map/api/v1/pings")
            .RequireAuthorization("TenantMapAccess"); // Require Map permission for all ping APIs

        // List active pings
        group.MapGet("", GetActivePings);

        // Create a new ping
        group.MapPost("", CreatePing);
    }

    /// <summary>
    /// Get all active (non-expired) pings for the current tenant
    /// </summary>
    private static async Task<IResult> GetActivePings(
        HttpContext context,
        IPingService pingService)
    {
        if (!context.User.Identity?.IsAuthenticated ?? true)
            return Results.Unauthorized();

        try
        {
            var pings = await pingService.GetActiveForTenantAsync();
            return Results.Json(pings);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    /// <summary>
    /// Create a new ping with rate limiting (max 5 active pings per user)
    /// </summary>
    private static async Task<IResult> CreatePing(
        HttpContext context,
        [FromBody] CreatePingDto dto,
        IPingService pingService,
        IUpdateNotificationService updateNotificationService)
    {
        if (!context.User.Identity?.IsAuthenticated ?? true)
            return Results.Unauthorized();

        var username = context.User.Identity?.Name ?? string.Empty;

        try
        {
            // Create ping with rate limiting (max 5 per user)
            var ping = await pingService.CreateAsync(dto, username);

            // Notify all SSE clients in the same tenant
            updateNotificationService.NotifyPingCreated(ping);

            return Results.Created($"/map/api/v1/pings/{ping.Id}", ping);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("maximum"))
        {
            // User has reached ping limit
            return Results.StatusCode(429); // 429 Too Many Requests
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }
}
