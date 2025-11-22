using HnHMapperServer.Core.Models;

namespace HnHMapperServer.Core.Interfaces;

public interface IConfigRepository
{
    Task<Config> GetConfigAsync();
    Task SaveConfigAsync(Config config);
    Task<string?> GetValueAsync(string key);
    Task SetValueAsync(string key, string value);
    Task DeleteValueAsync(string key);

    // Global config methods (not tenant-scoped)
    Task<string?> GetGlobalValueAsync(string key);
    Task SetGlobalValueAsync(string key, string value);
}
