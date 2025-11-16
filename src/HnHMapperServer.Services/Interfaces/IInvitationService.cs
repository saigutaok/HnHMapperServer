using HnHMapperServer.Core.DTOs;

namespace HnHMapperServer.Services.Interfaces;

/// <summary>
/// Service interface for managing tenant invitations
/// </summary>
public interface IInvitationService
{
    Task<InvitationDto> CreateInvitationAsync(string tenantId, string createdBy);
    Task<InvitationDto?> GetInvitationAsync(string inviteCode);
    Task<ValidateInvitationDto> ValidateInvitationAsync(string inviteCode);
    Task<List<InvitationDto>> GetTenantInvitationsAsync(string tenantId);
    Task RevokeInvitationAsync(int invitationId);
    Task<InvitationDto> UseInvitationAsync(string inviteCode, string username);
}
