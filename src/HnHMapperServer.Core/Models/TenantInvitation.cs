namespace HnHMapperServer.Core.Models;

/// <summary>
/// Manages invitation codes and pending registrations
/// </summary>
public sealed class TenantInvitationEntity
{
    /// <summary>
    /// Auto-increment primary key
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to Tenants
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// GUID-based unique invite code
    /// </summary>
    public string InviteCode { get; set; } = string.Empty;

    /// <summary>
    /// Username of creator
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// ISO 8601 UTC timestamp when invitation was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// ISO 8601 UTC timestamp when invitation expires (7 days from creation)
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Username who used this invite
    /// </summary>
    public string? UsedBy { get; set; }

    /// <summary>
    /// ISO 8601 UTC timestamp when invite was used
    /// </summary>
    public DateTime? UsedAt { get; set; }

    /// <summary>
    /// Status: 'Active', 'Used', 'Expired', or 'Revoked'
    /// </summary>
    public string Status { get; set; } = "Active";

    /// <summary>
    /// Whether registration is pending approval (1) or not (0)
    /// </summary>
    public bool PendingApproval { get; set; } = false;
}
