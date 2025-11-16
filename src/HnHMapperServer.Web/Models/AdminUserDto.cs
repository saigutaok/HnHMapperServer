namespace HnHMapperServer.Web.Models;

public class AdminUserDto
{
    public string Username { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
    public List<string> Tokens { get; set; } = new();
}

public class CreateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
}

public class UpdateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
}

public class ChangeUserPasswordRequest
{
    public string Username { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class TokenDto
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
    public string Url { get; set; } = string.Empty;
}
