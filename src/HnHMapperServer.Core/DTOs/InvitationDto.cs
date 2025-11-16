namespace HnHMapperServer.Core.DTOs;

/// <summary>
/// DTO for tenant invitation
/// </summary>
public class InvitationDto
{
    public int Id { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public string TenantName { get; set; } = string.Empty;
    public string InviteCode { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string? UsedBy { get; set; }
    public DateTime? UsedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool PendingApproval { get; set; }
}

/// <summary>
/// DTO for creating a new invitation
/// </summary>
public class CreateInvitationDto
{
    public string TenantId { get; set; } = string.Empty;
}

/// <summary>
/// DTO for validating an invitation
/// </summary>
public class ValidateInvitationDto
{
    public bool IsValid { get; set; }
    public string? TenantId { get; set; }
    public string? TenantName { get; set; }
    public string? ErrorMessage { get; set; }
}
