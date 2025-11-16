using HnHMapperServer.Core.Models;

namespace HnHMapperServer.Core.Interfaces;

public interface IUserRepository
{
    Task<User?> GetUserAsync(string username);
    Task<User?> GetUserByTokenAsync(string token);
    Task SaveUserAsync(User user);
    Task DeleteUserAsync(string username);
    Task<List<string>> GetAllUsernamesAsync();
    Task<bool> UserExistsAsync(string username);
}
