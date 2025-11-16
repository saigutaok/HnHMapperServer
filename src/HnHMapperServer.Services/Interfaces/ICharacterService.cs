using HnHMapperServer.Core.Models;
using System.Collections.Concurrent;

namespace HnHMapperServer.Services.Interfaces;

public interface ICharacterService
{
    /// <summary>
    /// Updates or adds a character to the tracking system
    /// </summary>
    void UpdateCharacter(string key, Character character);

    /// <summary>
    /// Gets all active characters for a specific tenant
    /// </summary>
    List<Character> GetAllCharacters(string tenantId);

    /// <summary>
    /// Removes stale characters (older than specified timeout) for a specific tenant
    /// </summary>
    void CleanupStaleCharacters(TimeSpan timeout, string tenantId);

    /// <summary>
    /// Gets the internal character dictionary (for cleanup service)
    /// </summary>
    ConcurrentDictionary<(string tenantId, string characterKey), Character> GetCharactersDictionary();
}
