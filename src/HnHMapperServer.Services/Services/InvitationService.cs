using HnHMapperServer.Core.DTOs;
using HnHMapperServer.Core.Interfaces;
using HnHMapperServer.Core.Models;
using HnHMapperServer.Infrastructure.Data;
using HnHMapperServer.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HnHMapperServer.Services.Services;

public class InvitationService : IInvitationService
{
    private readonly ITenantInvitationRepository _invitationRepository;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<InvitationService> _logger;

    public InvitationService(
        ITenantInvitationRepository invitationRepository,
        ApplicationDbContext context,
        ILogger<InvitationService> logger)
    {
        _invitationRepository = invitationRepository;
        _context = context;
        _logger = logger;
    }

    public async Task<InvitationDto> CreateInvitationAsync(string tenantId, string createdBy)
    {
        // Verify tenant exists and is active
        var tenant = await _context.Tenants.FindAsync(tenantId);
        if (tenant == null)
        {
            throw new ArgumentException($"Tenant {tenantId} not found");
        }

        if (!tenant.IsActive)
        {
            throw new InvalidOperationException($"Tenant {tenantId} is not active");
        }

        // Create invitation
        var invitation = new TenantInvitationEntity
        {
            TenantId = tenantId,
            InviteCode = Guid.NewGuid().ToString(),
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            Status = "Active",
            PendingApproval = false
        };

        var created = await _invitationRepository.CreateAsync(invitation);

        _logger.LogInformation("Created invitation {InviteCode} for tenant {TenantId} by {CreatedBy}",
            created.InviteCode, tenantId, createdBy);

        return MapToDto(created, tenant.Name);
    }

    public async Task<InvitationDto?> GetInvitationAsync(string inviteCode)
    {
        var invitation = await _invitationRepository.GetByInviteCodeAsync(inviteCode);
        if (invitation == null)
        {
            return null;
        }

        var tenant = await _context.Tenants.FindAsync(invitation.TenantId);
        return MapToDto(invitation, tenant?.Name ?? invitation.TenantId);
    }

    public async Task<ValidateInvitationDto> ValidateInvitationAsync(string inviteCode)
    {
        var invitation = await _invitationRepository.GetByInviteCodeAsync(inviteCode);

        if (invitation == null)
        {
            return new ValidateInvitationDto
            {
                IsValid = false,
                ErrorMessage = "Invitation code not found"
            };
        }

        if (invitation.Status != "Active")
        {
            return new ValidateInvitationDto
            {
                IsValid = false,
                ErrorMessage = $"Invitation is {invitation.Status.ToLower()}"
            };
        }

        if (invitation.ExpiresAt < DateTime.UtcNow)
        {
            return new ValidateInvitationDto
            {
                IsValid = false,
                ErrorMessage = "Invitation has expired"
            };
        }

        var tenant = await _context.Tenants.FindAsync(invitation.TenantId);
        if (tenant == null || !tenant.IsActive)
        {
            return new ValidateInvitationDto
            {
                IsValid = false,
                ErrorMessage = "Tenant is not active"
            };
        }

        return new ValidateInvitationDto
        {
            IsValid = true,
            TenantId = tenant.Id,
            TenantName = tenant.Name
        };
    }

    public async Task<List<InvitationDto>> GetTenantInvitationsAsync(string tenantId)
    {
        var invitations = await _invitationRepository.GetByTenantIdAsync(tenantId);
        var tenant = await _context.Tenants.FindAsync(tenantId);
        var tenantName = tenant?.Name ?? tenantId;

        return invitations.Select(i => MapToDto(i, tenantName)).ToList();
    }

    public async Task RevokeInvitationAsync(int invitationId)
    {
        var invitation = await _invitationRepository.GetByIdAsync(invitationId);
        if (invitation == null)
        {
            throw new ArgumentException($"Invitation {invitationId} not found");
        }

        if (invitation.Status == "Used")
        {
            throw new InvalidOperationException("Cannot revoke a used invitation");
        }

        invitation.Status = "Revoked";
        await _invitationRepository.UpdateAsync(invitation);

        _logger.LogInformation("Revoked invitation {InvitationId} for tenant {TenantId}",
            invitationId, invitation.TenantId);
    }

    public async Task<InvitationDto> UseInvitationAsync(string inviteCode, string username)
    {
        var invitation = await _invitationRepository.GetByInviteCodeAsync(inviteCode);
        if (invitation == null)
        {
            throw new ArgumentException($"Invitation code {inviteCode} not found");
        }

        // Validate invitation
        var validation = await ValidateInvitationAsync(inviteCode);
        if (!validation.IsValid)
        {
            throw new InvalidOperationException(validation.ErrorMessage ?? "Invalid invitation");
        }

        // Mark as used
        invitation.Status = "Used";
        invitation.UsedBy = username;
        invitation.UsedAt = DateTime.UtcNow;
        invitation.PendingApproval = true;

        await _invitationRepository.UpdateAsync(invitation);

        _logger.LogInformation("Invitation {InviteCode} used by {Username} for tenant {TenantId}",
            inviteCode, username, invitation.TenantId);

        var tenant = await _context.Tenants.FindAsync(invitation.TenantId);
        return MapToDto(invitation, tenant?.Name ?? invitation.TenantId);
    }

    private static InvitationDto MapToDto(TenantInvitationEntity entity, string tenantName)
    {
        return new InvitationDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            TenantName = tenantName,
            InviteCode = entity.InviteCode,
            CreatedBy = entity.CreatedBy,
            CreatedAt = entity.CreatedAt,
            ExpiresAt = entity.ExpiresAt,
            UsedBy = entity.UsedBy,
            UsedAt = entity.UsedAt,
            Status = entity.Status,
            PendingApproval = entity.PendingApproval
        };
    }
}
