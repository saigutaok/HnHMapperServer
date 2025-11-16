namespace HnHMapperServer.Web.Models;

/// <summary>
/// DTO for tenant invitation information
/// </summary>
public class InvitationDto
{
    /// <summary>
    /// Invitation ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Tenant ID this invitation is for
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Tenant display name
    /// </summary>
    public string TenantName { get; set; } = string.Empty;

    /// <summary>
    /// Invitation code (GUID)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Username of the user who created the invitation
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// When the invitation was created (UTC)
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the invitation expires (7 days from creation)
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Invitation status (Active, Used, Expired, Revoked)
    /// </summary>
    public string Status { get; set; } = "Active";

    /// <summary>
    /// Username of the user who used this invitation (null if not used)
    /// </summary>
    public string? UsedBy { get; set; }

    /// <summary>
    /// When the invitation was used (null if not used)
    /// </summary>
    public DateTime? UsedAt { get; set; }

    /// <summary>
    /// Whether the user who registered is pending approval
    /// </summary>
    public bool PendingApproval { get; set; }

    /// <summary>
    /// Calculates if invitation is expired
    /// </summary>
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    /// <summary>
    /// Calculates if invitation is still valid
    /// </summary>
    public bool IsValid => Status == "Active" && !IsExpired;

    /// <summary>
    /// Generates full invitation URL (to be set by controller/page)
    /// </summary>
    public string InvitationUrl { get; set; } = string.Empty;
}
