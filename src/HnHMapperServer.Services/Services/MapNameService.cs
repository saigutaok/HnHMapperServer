using HnHMapperServer.Core.Constants;
using HnHMapperServer.Infrastructure.Data;
using HnHMapperServer.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HnHMapperServer.Services.Services;

/// <summary>
/// Generates unique map identifiers in format: icon1-icon2-number (e.g., "arrow-wagon-4273")
/// Uses icon names extracted from actual game icon files.
/// Map names are unique within tenant scope.
/// </summary>
public class MapNameService : IMapNameService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<MapNameService> _logger;

    public MapNameService(ApplicationDbContext db, ILogger<MapNameService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Generates a unique map identifier with random 4-digit number
    /// </summary>
    /// <param name="tenantId">The tenant ID to ensure uniqueness within tenant scope</param>
    /// <returns>Unique map identifier in format: icon1-icon2-number (e.g., "arrow-wagon-4273")</returns>
    public async Task<string> GenerateUniqueIdentifierAsync(string tenantId)
    {
        const int maxAttempts = 100;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            // Pick two different random icons from the 2227 available names
            var icon1 = IconNames.Names[Random.Shared.Next(IconNames.Names.Length)];
            var icon2 = IconNames.Names[Random.Shared.Next(IconNames.Names.Length)];

            // Ensure icons are different
            while (icon2 == icon1)
            {
                icon2 = IconNames.Names[Random.Shared.Next(IconNames.Names.Length)];
            }

            // Generate random 4-digit number (1000-9999)
            var randomNumber = Random.Shared.Next(1000, 10000);
            var identifier = $"{icon1}-{icon2}-{randomNumber}";

            // Verify unique within tenant (race condition safety)
            var exists = await _db.Maps.AnyAsync(m => m.Name == identifier && m.TenantId == tenantId);
            if (!exists)
            {
                _logger.LogInformation("Generated map identifier for tenant {TenantId}: {Identifier}",
                    tenantId, identifier);
                return identifier;
            }

            _logger.LogWarning("Collision detected for map {Identifier} in tenant {TenantId}, retrying... (attempt {Attempt}/{MaxAttempts})",
                identifier, tenantId, attempt + 1, maxAttempts);
        }

        throw new InvalidOperationException(
            $"Failed to generate unique map identifier for tenant {tenantId} after {maxAttempts} attempts");
    }
}
