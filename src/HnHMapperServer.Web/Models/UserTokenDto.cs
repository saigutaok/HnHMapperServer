namespace HnHMapperServer.Web.Models;

public class UserTokenDto
{
    public string Value { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
    public string Url { get; set; } = string.Empty;
}
