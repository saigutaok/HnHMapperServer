using Microsoft.Extensions.Logging;

namespace HnHMapperServer.Infrastructure.Data;

public class DatabaseSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(ApplicationDbContext context, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        // Identity roles and users are seeded in the API layer using UserManager/RoleManager.
        // This infrastructure seeder intentionally performs no data mutations.
        _logger.LogInformation("DatabaseSeeder: no-op. Identity seeding happens in API startup.");
        await Task.CompletedTask;
    }
}
