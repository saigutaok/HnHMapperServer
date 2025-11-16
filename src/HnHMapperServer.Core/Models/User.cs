namespace HnHMapperServer.Core.Models;

/// <summary>
/// Represents a user in the system
/// </summary>
public class User
{
    public string Username { get; set; } = string.Empty;
    public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
    public List<string> Tokens { get; set; } = new();
}
