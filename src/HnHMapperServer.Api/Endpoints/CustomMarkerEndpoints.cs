using System.Security.Claims;
using HnHMapperServer.Core.DTOs;
using HnHMapperServer.Core.Enums;
using HnHMapperServer.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using HnHMapperServer.Core.Extensions;
using HnHMapperServer.Core.Constants;

namespace HnHMapperServer.Api.Endpoints;

/// <summary>
/// Custom marker API endpoints
/// </summary>
public static class CustomMarkerEndpoints
{
    public static void MapCustomMarkerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/map/api/v1/custom-markers")
            .RequireAuthorization("TenantMapAccess"); // Require Map permission for all custom marker APIs

        // List custom markers for a map
        group.MapGet("", GetCustomMarkers);

        // Get a single custom marker
        group.MapGet("{id:int}", GetCustomMarkerById);

        // Create a new custom marker
        group.MapPost("", CreateCustomMarker)
            .RequireAuthorization("TenantMarkersAccess"); // Require Markers permission to create markers

        // Update an existing custom marker
        group.MapPut("{id:int}", UpdateCustomMarker);

        // Delete a custom marker
        group.MapDelete("{id:int}", DeleteCustomMarker);

        // Get available icons
        app.MapGet("/map/api/v1/custom-marker-icons", GetAvailableIcons)
            .RequireAuthorization("TenantMapAccess");
    }

    /// <summary>
    /// Get all custom markers for a specific map
    /// </summary>
    private static async Task<IResult> GetCustomMarkers(
        HttpContext context,
        [FromQuery] int? mapId,
        ICustomMarkerService customMarkerService)
    {
        if (!context.User.Identity?.IsAuthenticated ?? true)
            return Results.Unauthorized();

        // Map permission already enforced by TenantMapAccess policy

        // mapId is required
        if (!mapId.HasValue)
        {
            return Results.BadRequest(new { error = "mapId query parameter is required" });
        }

        var username = context.User.Identity?.Name ?? string.Empty;
        var isAdmin = HasPermission(context.User, Permission.Writer);

        try
        {
            var markers = await customMarkerService.GetByMapIdAsync(mapId.Value, username, isAdmin);
            return Results.Json(markers);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    /// <summary>
    /// Get a single custom marker by ID
    /// </summary>
    private static async Task<IResult> GetCustomMarkerById(
        HttpContext context,
        int id,
        ICustomMarkerService customMarkerService)
    {
        if (!context.User.Identity?.IsAuthenticated ?? true)
            return Results.Unauthorized();

        // Map permission already enforced by TenantMapAccess policy

        var username = context.User.Identity?.Name ?? string.Empty;
        var isAdmin = HasPermission(context.User, Permission.Writer);

        try
        {
            var marker = await customMarkerService.GetByIdAsync(id, username, isAdmin);
            if (marker == null)
            {
                return Results.NotFound(new { error = $"Custom marker with ID {id} not found" });
            }

            return Results.Json(marker);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    /// <summary>
    /// Create a new custom marker
    /// </summary>
    private static async Task<IResult> CreateCustomMarker(
        HttpContext context,
        [FromBody] CreateCustomMarkerDto dto,
        ICustomMarkerService customMarkerService,
        IUpdateNotificationService updateNotificationService)
    {
        // Authorization already enforced by .RequireAuthorization("TenantMarkersAccess") at route level
        if (!context.User.Identity?.IsAuthenticated ?? true)
            return Results.Unauthorized();

        var username = context.User.Identity?.Name ?? string.Empty;

        try
        {
            var created = await customMarkerService.CreateAsync(dto, username);

            // Extract tenant ID from context for SSE event
            var tenantId = context.Items["TenantId"] as string ?? string.Empty;

            // Publish SSE event for real-time updates
            var eventDto = new CustomMarkerEventDto
            {
                Id = created.Id,
                MapId = created.MapId,
                GridId = $"{created.CoordX}_{created.CoordY}", // Format consistent with game grids
                CoordX = created.CoordX,
                CoordY = created.CoordY,
                X = created.X,
                Y = created.Y,
                Title = created.Title,
                Icon = created.Icon,
                CreatedBy = created.CreatedBy,
                PlacedAt = created.PlacedAt,
                Hidden = created.Hidden,
                TenantId = tenantId
            };

            updateNotificationService.NotifyCustomMarkerCreated(eventDto);

            return Results.Created($"/map/api/v1/custom-markers/{created.Id}", created);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    /// <summary>
    /// Update an existing custom marker
    /// </summary>
    private static async Task<IResult> UpdateCustomMarker(
        HttpContext context,
        int id,
        [FromBody] UpdateCustomMarkerDto dto,
        ICustomMarkerService customMarkerService,
        IUpdateNotificationService updateNotificationService)
    {
        if (!context.User.Identity?.IsAuthenticated ?? true)
            return Results.Unauthorized();

        var username = context.User.Identity?.Name ?? string.Empty;
        var isAdmin = HasPermission(context.User, Permission.Writer);

        try
        {
            var updated = await customMarkerService.UpdateAsync(id, dto, username, isAdmin);

            // Extract tenant ID from context for SSE event
            var tenantId = context.Items["TenantId"] as string ?? string.Empty;

            // Publish SSE event for real-time updates
            var eventDto = new CustomMarkerEventDto
            {
                Id = updated.Id,
                MapId = updated.MapId,
                GridId = $"{updated.CoordX}_{updated.CoordY}",
                CoordX = updated.CoordX,
                CoordY = updated.CoordY,
                X = updated.X,
                Y = updated.Y,
                Title = updated.Title,
                Icon = updated.Icon,
                CreatedBy = updated.CreatedBy,
                PlacedAt = updated.PlacedAt,
                Hidden = updated.Hidden,
                TenantId = tenantId
            };

            updateNotificationService.NotifyCustomMarkerUpdated(eventDto);

            return Results.Ok(updated);
        }
        catch (KeyNotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Results.StatusCode(403); // Forbidden
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

    /// <summary>
    /// Delete a custom marker
    /// </summary>
    private static async Task<IResult> DeleteCustomMarker(
        HttpContext context,
        int id,
        ICustomMarkerService customMarkerService,
        IUpdateNotificationService updateNotificationService)
    {
        if (!context.User.Identity?.IsAuthenticated ?? true)
            return Results.Unauthorized();

        var username = context.User.Identity?.Name ?? string.Empty;
        var isAdmin = HasPermission(context.User, Permission.Writer);

        try
        {
            await customMarkerService.DeleteAsync(id, username, isAdmin);

            // Extract tenant ID from context for SSE event
            var tenantId = context.Items["TenantId"] as string ?? string.Empty;

            // Publish SSE event for real-time updates
            var deleteEvent = new CustomMarkerDeleteEventDto
            {
                Id = id,
                TenantId = tenantId
            };
            updateNotificationService.NotifyCustomMarkerDeleted(deleteEvent);

            return Results.NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Results.StatusCode(403); // Forbidden
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    /// <summary>
    /// Get list of available marker icons
    /// </summary>
    private static async Task<IResult> GetAvailableIcons(
        HttpContext context,
        ICustomMarkerService customMarkerService)
    {
        if (!context.User.Identity?.IsAuthenticated ?? true)
            return Results.Unauthorized();

        // Map permission already enforced by TenantMapAccess policy

        try
        {
            var icons = await customMarkerService.GetAvailableIconsAsync();
            return Results.Json(icons);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    /// <summary>
    /// Check if user has specific tenant permission
    /// </summary>
    private static bool HasPermission(ClaimsPrincipal user, Permission permission)
    {
        // SuperAdmin bypasses all permission checks
        if (user.IsInRole(AuthorizationConstants.Roles.SuperAdmin))
            return true;

        var permissionValue = permission.ToClaimValue();
        return user.Claims.Any(c =>
            c.Type == AuthorizationConstants.ClaimTypes.TenantPermission &&
            c.Value.Equals(permissionValue, StringComparison.OrdinalIgnoreCase));
    }
}



