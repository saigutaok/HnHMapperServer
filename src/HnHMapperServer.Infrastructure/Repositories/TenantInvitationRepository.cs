using HnHMapperServer.Core.Interfaces;
using HnHMapperServer.Core.Models;
using HnHMapperServer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HnHMapperServer.Infrastructure.Repositories;

public class TenantInvitationRepository : ITenantInvitationRepository
{
    private readonly ApplicationDbContext _context;

    public TenantInvitationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TenantInvitationEntity?> GetByIdAsync(int id)
    {
        return await _context.TenantInvitations
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<TenantInvitationEntity?> GetByInviteCodeAsync(string inviteCode)
    {
        return await _context.TenantInvitations
            .FirstOrDefaultAsync(i => i.InviteCode == inviteCode);
    }

    public async Task<List<TenantInvitationEntity>> GetByTenantIdAsync(string tenantId)
    {
        return await _context.TenantInvitations
            .Where(i => i.TenantId == tenantId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<TenantInvitationEntity>> GetPendingApprovalsByTenantIdAsync(string tenantId)
    {
        return await _context.TenantInvitations
            .Where(i => i.TenantId == tenantId &&
                       i.Status == "Used" &&
                       i.PendingApproval)
            .OrderByDescending(i => i.UsedAt)
            .ToListAsync();
    }

    public async Task<TenantInvitationEntity> CreateAsync(TenantInvitationEntity invitation)
    {
        _context.TenantInvitations.Add(invitation);
        await _context.SaveChangesAsync();
        return invitation;
    }

    public async Task UpdateAsync(TenantInvitationEntity invitation)
    {
        _context.TenantInvitations.Update(invitation);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var invitation = await GetByIdAsync(id);
        if (invitation != null)
        {
            _context.TenantInvitations.Remove(invitation);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<TenantInvitationEntity>> GetExpiredActiveInvitationsAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.TenantInvitations
            .Where(i => i.Status == "Active" && i.ExpiresAt < now)
            .ToListAsync();
    }
}
