namespace HnHMapperServer.Web.Models;

/// <summary>
/// DTO for users pending approval in a tenant
/// </summary>
public class PendingUserDto
{
    /// <summary>
    /// User ID
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Username
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// When the user requested to join (registration timestamp)
    /// </summary>
    public DateTime RequestedAt { get; set; }

    /// <summary>
    /// Invitation code used to register
    /// </summary>
    public string? InvitationCode { get; set; }

    /// <summary>
    /// Days until auto-rejection (7 days from registration)
    /// </summary>
    public int DaysUntilAutoRejection
    {
        get
        {
            var autoRejectDate = RequestedAt.AddDays(7);
            var daysRemaining = (autoRejectDate - DateTime.UtcNow).Days;
            return Math.Max(0, daysRemaining);
        }
    }

    /// <summary>
    /// Whether auto-rejection warning should be shown
    /// </summary>
    public bool ShowAutoRejectionWarning => DaysUntilAutoRejection <= 2;
}
