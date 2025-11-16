using HnHMapperServer.Core.DTOs;
using HnHMapperServer.Core.Enums;
using HnHMapperServer.Infrastructure.Data;
using HnHMapperServer.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HnHMapperServer.Api.Endpoints;

/// <summary>
/// Endpoints for managing tenant invitations
/// </summary>
public static class InvitationEndpoints
{
    public static void MapInvitationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/invitations");

        // Public endpoint to validate invitation code
        group.MapGet("/validate/{code}", ValidateInvitation);

        // Public endpoint to get invitation details
        group.MapGet("/{code}", GetInvitation);

        // Tenant-scoped endpoints (require authentication)
        var tenantGroup = app.MapGroup("/api/tenants/{tenantId}/invitations");

        tenantGroup.MapPost("", CreateInvitation)
            .RequireAuthorization();

        tenantGroup.MapGet("", GetTenantInvitations)
            .RequireAuthorization();

        tenantGroup.MapDelete("/{invitationId:int}", RevokeInvitation)
            .RequireAuthorization();
    }

    private static async Task<IResult> ValidateInvitation(
        [FromRoute] string code,
        [FromServices] IInvitationService invitationService)
    {
        var result = await invitationService.ValidateInvitationAsync(code);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetInvitation(
        [FromRoute] string code,
        [FromServices] IInvitationService invitationService)
    {
        var invitation = await invitationService.GetInvitationAsync(code);
        if (invitation == null)
        {
            return Results.NotFound(new { error = "Invitation not found" });
        }

        return Results.Ok(invitation);
    }

    private static async Task<IResult> CreateInvitation(
        [FromRoute] string tenantId,
        HttpContext context,
        [FromServices] ApplicationDbContext db,
        [FromServices] IInvitationService invitationService,
        ILogger<Program> logger)
    {
        // Get current user from claims
        var username = context.User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
        {
            return Results.Unauthorized();
        }

        // Get user ID from AspNetUsers table
        var user = await db.Users.FirstOrDefaultAsync(u => u.UserName == username);
        if (user == null)
        {
            logger.LogWarning("User {Username} not found in database", username);
            return Results.Unauthorized();
        }

        // Check if user is TenantAdmin for this tenant
        var tenantUser = await db.TenantUsers
            .FirstOrDefaultAsync(tu => tu.TenantId == tenantId && tu.UserId == user.Id);

        if (tenantUser == null)
        {
            logger.LogWarning("User {Username} is not a member of tenant {TenantId}", username, tenantId);
            return Results.Forbid();
        }

        if (tenantUser.Role != TenantRole.TenantAdmin)
        {
            logger.LogWarning("User {Username} is not a TenantAdmin for tenant {TenantId}", username, tenantId);
            return Results.Forbid();
        }

        try
        {
            var invitation = await invitationService.CreateInvitationAsync(tenantId, username);
            return Results.Ok(invitation);
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Failed to create invitation for tenant {TenantId}", tenantId);
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Tenant {TenantId} is not active", tenantId);
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> GetTenantInvitations(
        [FromRoute] string tenantId,
        [FromServices] IInvitationService invitationService)
    {
        var invitations = await invitationService.GetTenantInvitationsAsync(tenantId);
        return Results.Ok(invitations);
    }

    private static async Task<IResult> RevokeInvitation(
        [FromRoute] string tenantId,
        [FromRoute] int invitationId,
        [FromServices] IInvitationService invitationService,
        ILogger<Program> logger)
    {
        try
        {
            await invitationService.RevokeInvitationAsync(invitationId);
            return Results.Ok(new { message = "Invitation revoked successfully" });
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Invitation {InvitationId} not found", invitationId);
            return Results.NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Cannot revoke invitation {InvitationId}", invitationId);
            return Results.BadRequest(new { error = ex.Message });
        }
    }
}
