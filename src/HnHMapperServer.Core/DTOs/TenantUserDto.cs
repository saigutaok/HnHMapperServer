namespace HnHMapperServer.Core.DTOs;

/// <summary>
/// DTO for viewing tenant user details with their role and permissions
/// </summary>
public class TenantUserDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // TenantAdmin or TenantUser
    public List<string> Permissions { get; set; } = new();
    public DateTime JoinedAt { get; set; }
    public bool PendingApproval { get; set; }
}

/// <summary>
/// DTO for updating tenant user permissions
/// </summary>
public class UpdateTenantUserPermissionsDto
{
    public List<string> Permissions { get; set; } = new();
}
