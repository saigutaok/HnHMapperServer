using HnHMapperServer.Core.Constants;
using HnHMapperServer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HnHMapperServer.Services.Services;

/// <summary>
/// Generates unique tenant identifiers in format: icon1-icon2-number (e.g., "arrow-wagon-4273")
/// Uses icon names extracted from actual game icon files.
/// </summary>
public class TenantNameService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<TenantNameService> _logger;

    public TenantNameService(ApplicationDbContext db, ILogger<TenantNameService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Generates a unique tenant identifier with random 4-digit number
    /// </summary>
    /// <returns>Unique tenant identifier in format: icon1-icon2-number (e.g., "arrow-wagon-4273")</returns>
    public async Task<string> GenerateUniqueIdentifierAsync()
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

            // Verify unique (race condition safety)
            var exists = await _db.Tenants.AnyAsync(t => t.Id == identifier);
            if (!exists)
            {
                _logger.LogInformation("Generated tenant identifier: {Identifier}", identifier);
                return identifier;
            }

            _logger.LogWarning("Collision detected for {Identifier}, retrying... (attempt {Attempt}/{MaxAttempts})",
                identifier, attempt + 1, maxAttempts);
        }

        throw new InvalidOperationException(
            $"Failed to generate unique tenant identifier after {maxAttempts} attempts");
    }
}
