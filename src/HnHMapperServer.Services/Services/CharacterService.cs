using HnHMapperServer.Core.Models;
using HnHMapperServer.Core.DTOs;
using HnHMapperServer.Services.Interfaces;
using System.Collections.Concurrent;

namespace HnHMapperServer.Services.Services;

public class CharacterService : ICharacterService
{
    // Use composite key (tenantId, characterKey) for tenant isolation
    private readonly ConcurrentDictionary<(string tenantId, string characterKey), Character> _characters = new();
    private readonly IUpdateNotificationService _updateNotificationService;

    public CharacterService(IUpdateNotificationService updateNotificationService)
    {
        _updateNotificationService = updateNotificationService;
    }

    public void UpdateCharacter(string key, Character character)
    {
        // Validate tenant ID is provided
        if (string.IsNullOrEmpty(character.TenantId))
        {
            throw new ArgumentException("Character must have a TenantId", nameof(character));
        }

        bool isNewOrChanged = false;
        var compositeKey = (character.TenantId, key);

        _characters.AddOrUpdate(compositeKey, 
            // Add factory - new character
            (k) => 
            {
                isNewOrChanged = true;
                return character;
            },
            // Update factory - existing character
            (k, existing) =>
            {
                // Calculate speed/rotation from position deltas if client didn't provide them
                if (character.Speed == 0 && character.Rotation == 0)
                {
                    var timeElapsed = (character.Updated - existing.Updated).TotalSeconds;

                    // Only calculate if enough time has elapsed (avoid noise from rapid updates)
                    if (timeElapsed >= 0.1) // Minimum 100ms between updates
                    {
                        var deltaX = character.Position.X - existing.Position.X;
                        var deltaY = character.Position.Y - existing.Position.Y;

                        // Speed: distance traveled per second (pixels/second)
                        var distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
                        character.Speed = (int)(distance / timeElapsed);

                        // Rotation: angle of movement in degrees (0-360)
                        // Game convention: 0° = North, 90° = East, 180° = South, 270° = West
                        // Y-axis increases downward (screen coordinates)
                        if (distance > 0) // Only calculate if character actually moved
                        {
                            var angleRadians = Math.Atan2(deltaY, deltaX);
                            var angleDegrees = angleRadians * 180.0 / Math.PI;
                            // Convert from math angles (0°=East, Y-down) to game angles (0°=North)
                            character.Rotation = (int)((angleDegrees + 90 + 360) % 360);
                        }
                    }
                }

                // Store current position/time as previous for next calculation
                character.PreviousPosition = existing.Position;
                character.PreviousUpdated = existing.Updated;

                // Prefer "player" type, then non-"unknown" type, then any update
                if (existing.Type == "player")
                {
                    if (character.Type == "player")
                    {
                        isNewOrChanged = HasCharacterChanged(existing, character);
                        return character;
                    }
                    else
                    {
                        character.Type = existing.Type;
                        isNewOrChanged = HasCharacterChanged(existing, character);
                        return character;
                    }
                }
                else if (existing.Type != "unknown")
                {
                    if (character.Type != "unknown")
                    {
                        isNewOrChanged = HasCharacterChanged(existing, character);
                        return character;
                    }
                    else
                    {
                        character.Type = existing.Type;
                        isNewOrChanged = HasCharacterChanged(existing, character);
                        return character;
                    }
                }
                isNewOrChanged = HasCharacterChanged(existing, character);
                return character;
            });

        // Publish delta if there was a meaningful change
        if (isNewOrChanged)
        {
            var delta = new CharacterDeltaDto
            {
                TenantId = character.TenantId,
                Updates = new List<CharacterDto>
                {
                    new CharacterDto
                    {
                        Id = character.Id,
                        Name = character.Name,
                        Map = character.Map,
                        X = character.Position.X,
                        Y = character.Position.Y,
                        Type = character.Type,
                        Rotation = character.Rotation,
                        Speed = character.Speed
                    }
                },
                Deletions = new List<int>()
            };

            _updateNotificationService.NotifyCharacterDelta(delta);
        }
    }

    /// <summary>
    /// Checks if character has meaningful changes (position, map, type, rotation, speed)
    /// </summary>
    private bool HasCharacterChanged(Character existing, Character updated)
    {
        return existing.Map != updated.Map ||
               existing.Position.X != updated.Position.X ||
               existing.Position.Y != updated.Position.Y ||
               existing.Type != updated.Type ||
               existing.Rotation != updated.Rotation ||
               existing.Speed != updated.Speed;
    }

    public List<Character> GetAllCharacters(string tenantId)
    {
        if (string.IsNullOrEmpty(tenantId))
        {
            return new List<Character>();
        }

        return _characters
            .Where(kvp => kvp.Key.tenantId == tenantId)
            .Select(kvp => kvp.Value)
            .ToList();
    }

    public void CleanupStaleCharacters(TimeSpan timeout, string tenantId)
    {
        if (string.IsNullOrEmpty(tenantId))
        {
            return;
        }

        var cutoffTime = DateTime.UtcNow - timeout;
        var staleEntries = _characters
            .Where(kvp => kvp.Key.tenantId == tenantId && kvp.Value.Updated < cutoffTime)
            .ToList();

        if (staleEntries.Count == 0)
            return;

        var deletedIds = new List<int>();

        foreach (var entry in staleEntries)
        {
            if (_characters.TryRemove(entry.Key, out var removed))
            {
                deletedIds.Add(removed.Id);
            }
        }

        // Publish deletions if any characters were removed
        if (deletedIds.Count > 0)
        {
            var delta = new CharacterDeltaDto
            {
                TenantId = tenantId,
                Updates = new List<CharacterDto>(),
                Deletions = deletedIds
            };
            _updateNotificationService.NotifyCharacterDelta(delta);
        }
    }

    public ConcurrentDictionary<(string tenantId, string characterKey), Character> GetCharactersDictionary()
    {
        return _characters;
    }
}
