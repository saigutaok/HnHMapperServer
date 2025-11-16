namespace HnHMapperServer.Services.Interfaces;

/// <summary>
/// Service for managing authentication tokens with tenant prefixes
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Creates a new authentication token with tenant prefix
    /// </summary>
    /// <param name="tenantId">Tenant ID (e.g., "warrior-shield-42")</param>
    /// <param name="userId">User ID who owns the token</param>
    /// <param name="name">User-friendly name for the token</param>
    /// <param name="scopes">Comma-separated scopes (e.g., "upload,map")</param>
    /// <returns>Full token with tenant prefix (e.g., "warrior-shield-42_abc123...")</returns>
    Task<string> CreateTokenAsync(string tenantId, string userId, string name, string scopes);

    /// <summary>
    /// Validates a token and extracts tenant and user information
    /// </summary>
    /// <param name="fullToken">Full token including tenant prefix</param>
    /// <returns>Validation result with tenant ID and user ID if valid</returns>
    Task<TokenValidationResult> ValidateTokenAsync(string fullToken);

    /// <summary>
    /// Lists all tokens for a specific tenant (tenant-scoped)
    /// </summary>
    /// <param name="tenantId">Tenant ID to filter by</param>
    /// <returns>List of tokens (without plaintext secrets)</returns>
    Task<List<TokenInfo>> GetTokensByTenantAsync(string tenantId);

    /// <summary>
    /// Revokes a token by ID
    /// </summary>
    /// <param name="tokenId">Token ID (GUID)</param>
    /// <param name="userId">User requesting revocation (for authorization)</param>
    /// <param name="isAdmin">Whether user is tenant admin</param>
    Task RevokeTokenAsync(string tokenId, string userId, bool isAdmin);
}

/// <summary>
/// Result of token validation
/// </summary>
public class TokenValidationResult
{
    public bool IsValid { get; set; }
    public string? TenantId { get; set; }
    public string? UserId { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Token information for display (no plaintext secret)
/// </summary>
public class TokenInfo
{
    public string Id { get; set; } = string.Empty;
    public string DisplayToken { get; set; } = string.Empty; // Full token with tenant prefix
    public string Name { get; set; } = string.Empty;
    public string Scopes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
}
