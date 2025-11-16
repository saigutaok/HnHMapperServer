using HnHMapperServer.Core.Models;

namespace HnHMapperServer.Core.Interfaces;

/// <summary>
/// Repository interface for tenant invitation operations
/// </summary>
public interface ITenantInvitationRepository
{
    Task<TenantInvitationEntity?> GetByIdAsync(int id);
    Task<TenantInvitationEntity?> GetByInviteCodeAsync(string inviteCode);
    Task<List<TenantInvitationEntity>> GetByTenantIdAsync(string tenantId);
    Task<List<TenantInvitationEntity>> GetPendingApprovalsByTenantIdAsync(string tenantId);
    Task<TenantInvitationEntity> CreateAsync(TenantInvitationEntity invitation);
    Task UpdateAsync(TenantInvitationEntity invitation);
    Task DeleteAsync(int id);
    Task<List<TenantInvitationEntity>> GetExpiredActiveInvitationsAsync();
}
